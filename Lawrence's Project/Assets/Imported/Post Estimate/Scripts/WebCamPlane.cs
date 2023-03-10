using UnityEngine;

public class WebCamPlane : MonoBehaviour
{
    public int cameraIndex = 0;
    private WebCamTexture webcamTexture;
    private Renderer planeRenderer;

    void Start()
    {
        planeRenderer = GetComponent<Renderer>();

        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.LogError("No cameras found");
            return;
        }

        webcamTexture = new WebCamTexture(devices[cameraIndex].name);
        planeRenderer.material.mainTexture = webcamTexture;
        webcamTexture.Play();
    }
}
