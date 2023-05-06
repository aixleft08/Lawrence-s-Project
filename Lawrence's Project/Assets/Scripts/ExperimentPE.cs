using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentPE : MonoBehaviour
{
    public bool start;
    Transform rightShoulder;
    Transform rightWrist;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(rightShoulder == null && PoseEstimator.Instance.ready && start)
        {
            rightShoulder = GameObject.Find("rightShoulder").transform;
        }
        if(rightWrist == null && PoseEstimator.Instance.ready && start)
        {
            rightWrist = GameObject.Find("rightWrist").transform;
        }

        if(rightShoulder != null && rightWrist != null && PoseEstimator.Instance.ready)
        {
            if(rightShoulder.position.x > rightWrist.position.x)
            {
                print("Swing");
            }
        }
    }
}
