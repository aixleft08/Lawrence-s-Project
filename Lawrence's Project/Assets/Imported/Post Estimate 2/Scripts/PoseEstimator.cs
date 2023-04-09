using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using System;
using UnityEngine.UI;

public class PoseEstimator : MonoBehaviour
{
    public enum EstimationType
    {
        MultiPose,
        SinglePose
    }
        
    public TMPro.TMP_InputField inputWidthField;
    public TMPro.TMP_InputField inputHeightField;

    public TMPro.TMP_InputField nmsField;

    [Tooltip("Switch between the available models")]
    public TMPro.TMP_Dropdown modelDropdown;

    [Tooltip("The requested webcam dimensions")]
    public Vector2Int webcamDims = new Vector2Int(1280, 720);

    [Tooltip("The requested webcam frame rate")]
    public int webcamFPS = 60;

    [Tooltip("The screen for viewing preprocessed images")]
    public Transform videoScreen;

    [Tooltip("The ComputeShader that will perform the model-specific preprocessing")]
    public ComputeShader posenetShader;

    [Tooltip("The dimensions of the image being fed to the model")]
    public Vector2Int imageDims = new Vector2Int(256, 256);

    [ListToPopup(typeof(PoseEstimator), "modelList")]
    public string Models = "";

    /// <summary>
    /// Stores a list of available onnx models
    /// </summary>
    public static List<string> modelList = new List<string>();

    [Tooltip("")]
    public List<PoseNetModel> poseNetModels;

    [Tooltip("The backend to use when performing inference")]
    public WorkerFactory.Type workerType = WorkerFactory.Type.Auto;

    [Tooltip("The type of pose estimation to be performed")]
    public EstimationType estimationType = EstimationType.SinglePose;

    [Tooltip("The maximum number of posees to estimate")]
    [Range(1, 20)]
    public int maxPoses = 20;

    [Tooltip("The score threshold for multipose estimation")]
    [Range(0, 1.0f)]
    public float scoreThreshold = 0.25f;

    [Tooltip("Non-maximum suppression part distance")]
    public int nmsRadius = 100;

    [Tooltip("The size of the pose skeleton key points")]
    public float pointScale = 10f;

    [Tooltip("The width of the pose skeleton lines")]
    public float lineWidth = 5f;

    [Tooltip("The minimum confidence level required to display the key point")]
    [Range(0, 100)]
    public int minConfidence = 70;


    // Live video input from a webcam
    private WebCamTexture webcamTexture;

    // The source video texture
    private RenderTexture videoTexture;

    // Target dimensions for model input
    private Vector2Int targetDims;

    // Used to scale the input image dimensions while maintaining aspect ratio
    private float aspectRatioScale;

    // The texture used to create input tensor
    private RenderTexture rTex;

    // Stores the input data for the model
    private Tensor input;

    // The interface used to execute the neural network
    private IWorker engine;

    // The name for the heatmap layer in the model asset
    private string heatmapLayer;

    // The name for the offsets layer in the model asset
    private string offsetsLayer;

    // The name for the forwards displacement layer in the model asset
    private string displacementFWDLayer;

    // The name for the backwards displacement layer in the model asset
    private string displacementBWDLayer;

    // The name for the Sigmoid layer that returns the heatmap predictions
    private string predictionLayer = "heatmap_predictions";

    // Stores the current estimated 2D keypoint locations in videoTexture
    private Utils.Keypoint[][] poses;

    // Array of pose skeletons
    private PoseSkeleton[] skeletons;

    // Stores the PoseNetModel currently in use
    private PoseNetModel currentModel;

    public static PoseEstimator Instance;
    public bool ready;

    void Awake()
    {
        Instance = this;
    }


    public void InitializeWebcam()
    {
        // Create a new WebCamTexture
        webcamTexture = new WebCamTexture(webcamDims.x, webcamDims.y);

        // Start the Camera
        webcamTexture.Play();
    }


    private void InitializeUI()
    {
        nmsField.SetTextWithoutNotify($"{nmsRadius}");

        modelDropdown.ClearOptions();
        // Add OpenVINO compute devices to dropdown
        modelDropdown.AddOptions(modelList);
        // Set the value for the dropdown to the current compute device
        modelDropdown.SetValueWithoutNotify(modelList.IndexOf(currentModel.modelAsset.name));
    }


    /// <summary>
    /// Prepares the videoScreen GameObject to display the chosen video source.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="mirrorScreen"></param>
    private void InitializeVideoScreen(int width, int height, bool mirrorScreen)
    {
        // Release temporary RenderTexture
        RenderTexture.ReleaseTemporary(videoTexture);
        // Create a new videoTexture using the current video dimensions
        videoTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGBHalf);

        if (mirrorScreen)
        {
            // Flip the VideoScreen around the Y-Axis
            videoScreen.rotation = Quaternion.Euler(0, 180, 0);
            // Invert the scale value for the Z-Axis
            videoScreen.localScale = new Vector3(videoScreen.localScale.x, videoScreen.localScale.y, -1f);
        }

        // Apply the new videoTexture to the VideoScreen Gameobject
        videoScreen.gameObject.GetComponent<MeshRenderer>().material.shader = Shader.Find("Unlit/Texture");
        videoScreen.gameObject.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", videoTexture);
        // Adjust the VideoScreen dimensions for the new videoTexture
        videoScreen.localScale = new Vector3(width, height, videoScreen.localScale.z);
        // Adjust the VideoScreen position for the new videoTexture
        videoScreen.position = new Vector3(width / 2, height / 2, 1);
    }

    /// <summary>
    /// Resizes and positions the in-game Camera to accommodate the video dimensions
    /// </summary>
    private void InitializeCamera()
    {
        // Get a reference to the Main Camera GameObject
        GameObject mainCamera = GameObject.Find("Main Camera");
        // Adjust the camera position to account for updates to the VideoScreen
        mainCamera.transform.position = new Vector3(videoTexture.width / 2, videoTexture.height / 2, -10f);
        // Render objects with no perspective (i.e. 2D)
        mainCamera.GetComponent<Camera>().orthographic = true;
        // Adjust the camera size to account for updates to the VideoScreen
        mainCamera.GetComponent<Camera>().orthographicSize = videoTexture.height / 2;
    }


    /// <summary>
    /// Updates the output layer names based on the selected model architecture
    /// and initializes the Barracuda inference engine with the selected model.
    /// </summary>
    private void InitializeBarracuda()
    {
        // The compiled model used for performing inference
        Model m_RunTimeModel;

        // Compile the model asset into an object oriented representation
        m_RunTimeModel = ModelLoader.Load(currentModel.modelAsset);

        // Get output layer names
        heatmapLayer = m_RunTimeModel.outputs[currentModel.heatmapLayerIndex];
        offsetsLayer = m_RunTimeModel.outputs[currentModel.offsetsLayerIndex];
        displacementFWDLayer = m_RunTimeModel.outputs[currentModel.displacementFWDLayerIndex];
        displacementBWDLayer = m_RunTimeModel.outputs[currentModel.displacementBWDLayerIndex];

        // Create a model builder to modify the m_RunTimeModel
        ModelBuilder modelBuilder = new ModelBuilder(m_RunTimeModel);

        // Add a new Sigmoid layer that takes the output of the heatmap layer
        modelBuilder.Sigmoid(predictionLayer, heatmapLayer);

        // Validate if backend is supported, otherwise use fallback type.
        workerType = WorkerFactory.ValidateType(workerType);

        // Create a worker that will execute the model with the selected backend
        engine = WorkerFactory.CreateWorker(workerType, modelBuilder.model);
    }


    /// <summary>
    /// Initialize pose skeletons
    /// </summary>
    private void InitializeSkeletons()
    {
        // Initialize the list of pose skeletons
        if (estimationType == EstimationType.SinglePose) maxPoses = 1;
        skeletons = new PoseSkeleton[maxPoses];

        // Populate the list of pose skeletons
        for (int i = 0; i < maxPoses; i++) skeletons[i] = new PoseSkeleton(pointScale, lineWidth);

        ready = true;
    }



    /// <summary>
    /// Initialize the input dimensions for the model
    /// </summary>
    private void InitializeInputDims()
    {
        // Prevent the input dimensions from going too low for the model
        imageDims.x = Mathf.Max(imageDims.x, 64);
        imageDims.y = Mathf.Max(imageDims.y, 64);

        // Update the input dimensions while maintaining the source aspect ratio
        if (imageDims.y != targetDims.y)
        {
            aspectRatioScale = (float)videoTexture.width / videoTexture.height;
            targetDims.x = (int)(imageDims.y * aspectRatioScale);
            imageDims.x = targetDims.x;
            targetDims.y = imageDims.y;
        }

        if (imageDims.x != targetDims.x)
        {
            aspectRatioScale = (float)videoTexture.height / videoTexture.width;
            targetDims.y = (int)(imageDims.x * aspectRatioScale);
            imageDims.y = targetDims.y;
            targetDims.x = imageDims.x;
        }

        RenderTexture.ReleaseTemporary(rTex);
        // Assign a temporary RenderTexture with the new dimensions
        rTex = RenderTexture.GetTemporary(imageDims.x, imageDims.y, 24, RenderTextureFormat.ARGBHalf);

        inputWidthField.text = $"{rTex.width}";
        inputHeightField.text = $"{rTex.height}";
    }


    /// <summary>
    /// Perform initialization steps
    /// </summary>
    private void InitializePoseEstimator()
    {
        // Initialize the Barracuda inference engine based on the selected model
        InitializeBarracuda();

        // Initialize pose skeletons
        InitializeSkeletons();

        // Initialize the videoScreen
        InitializeVideoScreen(webcamTexture.width, webcamTexture.height, true);

        // Adjust the camera based on the source video dimensions
        //InitializeCamera();

        // Initialize input dimensions
        InitializeInputDims();

        ready = true;
    }


    // Start is called before the first frame update
    void Start()
    {
        // Initialize webcam
        InitializeWebcam();

        // Get the names of the model assets
        foreach (PoseNetModel model in poseNetModels) modelList.Add(model.name);

        currentModel = poseNetModels[modelList.IndexOf(Models)];

        // Intialize GUI components
        InitializeUI();
    }

    /// <summary>
    /// Calls the appropriate preprocessing function to prepare
    /// the input for the selected model
    /// </summary>
    /// <param name="image"></param>
    private void ProcessImage(RenderTexture image)
    {
        // Define a temporary HDR RenderTexture
        RenderTexture result = RenderTexture.GetTemporary(image.width, image.height, 24, RenderTextureFormat.ARGBHalf);
        RenderTexture.active = result;

        // Apply preprocessing steps
        Graphics.Blit(image, result, currentModel.preprocessingMaterial);

        // Create a new Tensor
        input = new Tensor(result, channels: 3);
        RenderTexture.ReleaseTemporary(result);
    }

    /// <summary>
    /// Obtains the model output and either decodes single or mutlple poses
    /// </summary>
    /// <param name="engine"></param>
    private void ProcessOutput(IWorker engine)
    {
        // Get the model output
        Tensor heatmaps = engine.PeekOutput(predictionLayer);
        Tensor offsets = engine.PeekOutput(offsetsLayer);
        Tensor displacementFWD = engine.PeekOutput(displacementFWDLayer);
        Tensor displacementBWD = engine.PeekOutput(displacementBWDLayer);

        // Calculate the stride used to scale down the inputImage
        int stride = (imageDims.y - 1) / (heatmaps.shape.height - 1);
        stride -= (stride % 8);

        if (estimationType == EstimationType.SinglePose)
        {
            // Initialize the array of Keypoint arrays
            poses = new Utils.Keypoint[1][];

            // Determine the key point locations
            poses[0] = Utils.DecodeSinglePose(heatmaps, offsets, stride);
        }
        else
        {
            // Determine the key point locations
            poses = Utils.DecodeMultiplePoses(
                heatmaps, offsets,
                displacementFWD, displacementBWD,
                stride: stride, maxPoseDetections: maxPoses,
                scoreThreshold: scoreThreshold,
                nmsRadius: nmsRadius);
        }

        // Release the resources allocated for the output Tensors
        heatmaps.Dispose();
        offsets.Dispose();
        displacementFWD.Dispose();
        displacementBWD.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        // Skip the rest of the method if the webcam is not initialized
        if (webcamTexture.width <= 16) return;

        // Only perform initialization steps if the videoTexture has not been initialized
        if (!videoTexture) InitializePoseEstimator();

        // Copy webcamTexture to videoTexture
        Graphics.Blit(webcamTexture, videoTexture);

        // Copy the videoTexture data to rTex
        Graphics.Blit(videoTexture, rTex);

        // Prepare the input image to be fed to the selected model
        ProcessImage(rTex);

        // Execute neural network with the provided input
        engine.Execute(input);
        // Release GPU resources allocated for the Tensor
        input.Dispose();

        // Decode the keypoint coordinates from the model output
        ProcessOutput(engine);

        // Reinitialize pose skeletons
        if (maxPoses != skeletons.Length)
        {
            foreach (PoseSkeleton skeleton in skeletons)
            {
                skeleton.Cleanup();
            }

            // Initialize pose skeletons
            InitializeSkeletons();
            ready = true;
        }

        // The smallest dimension of the videoTexture
        int minDimension = Mathf.Min(videoTexture.width, videoTexture.height);

        // The value used to scale the key point locations up to the source resolution
        float scale = (float)minDimension / Mathf.Min(imageDims.x, imageDims.y);

        // Update the pose skeletons
        for (int i = 0; i < skeletons.Length; i++)
        {
            if (i <= poses.Length - 1)
            {
                skeletons[i].ToggleSkeleton(true);

                // Update the positions for the key point GameObjects
                skeletons[i].UpdateKeyPointPositions(poses[i], scale, videoTexture, true, minConfidence);
                skeletons[i].UpdateLines();
            }
            else
            {
                skeletons[i].ToggleSkeleton(false);
            }
        }

        Resources.UnloadUnusedAssets();
    }

    // OnDisable is called when the MonoBehavior becomes disabled or inactive
    private void OnDisable()
    {
        // Release the resources allocated for the inference engine
        engine.Dispose();
    }

    public void OnUserInput()
    {
        if (!Application.isPlaying) return;
        currentModel = poseNetModels[modelDropdown.value];
        Models = modelList[modelDropdown.value];

        // Reinitialize Barracuda with the selected model and backend
        engine.Dispose();

        Int32.TryParse(nmsField.text, out int nmsFieldValue);
        nmsRadius = nmsFieldValue;

        Int32.TryParse(inputWidthField.text, out int widthFieldValue);
        imageDims.x = widthFieldValue;

        Int32.TryParse(inputHeightField.text, out int heightFieldValue);
        imageDims.y = heightFieldValue;

        // Perform initialization steps
        InitializePoseEstimator();
    }
}
