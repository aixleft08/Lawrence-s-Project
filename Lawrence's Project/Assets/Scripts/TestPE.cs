using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPE : MonoBehaviour
{
    public Transform playerShoulder;
    public Transform playerElbow;   
    public Transform playerWrist;
    public Transform peWrist;
    public Transform peShoulder;
    public Transform peElbow;

    void Update()
    {
        playerShoulder.localRotation = Quaternion.Euler(peShoulder.localRotation.eulerAngles.x-30, peShoulder.localRotation.eulerAngles.y, peShoulder.localRotation.eulerAngles.z - 90);
        playerElbow.localRotation = Quaternion.Euler(-peElbow.localRotation.eulerAngles.z, peElbow.localRotation.eulerAngles.x-145, -peElbow.localRotation.eulerAngles.y);
        playerWrist.localRotation = Quaternion.Euler(peWrist.localRotation.eulerAngles.z, peWrist.localRotation.eulerAngles.x+180, peWrist.localRotation.eulerAngles.y);
    }
}
