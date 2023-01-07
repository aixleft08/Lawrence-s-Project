using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallHit : MonoBehaviour
{
    public Rigidbody ball;
    public Transform target;
    public Camera cam;
    public Transform indicator;
    public float force;
    public LayerMask layerMask;
    public Animator animator;

    RaycastHit hit;
    Ray ray;

    void Update()
    {

        ray = cam.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out hit, 100, layerMask))
        {
            target.position = hit.point + (hit.normal * 0.025f);
            target.rotation = Quaternion.FromToRotation (Vector3.up, hit.normal);
            Debug.DrawLine(cam.transform.position, hit.point, Color.red);
        }
        if(Physics.Raycast(ball.position, Vector3.down, out hit, 100, layerMask))
        {
            indicator.position = hit.point + (hit.normal * 0.025f);
            indicator.rotation = Quaternion.FromToRotation (Vector3.up, hit.normal);
            Debug.DrawLine(cam.transform.position, hit.point, Color.red);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Swing") && other.transform.tag == "Ball")
        {
            ball.velocity = Vector3.zero;
            ball.AddForce(-(ball.position-target.position) * force, ForceMode.Impulse);
        }
    }
}
