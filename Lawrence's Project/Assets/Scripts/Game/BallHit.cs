using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BallHit : MonoBehaviour
{
    public Rigidbody ball;
    public Transform target;
    public Camera cam;
    public Transform indicator;

    [Space(20)]
    public float force;
    public float forceChargeSpeed;
    public float maxForce;
    public float minForce;
    public Image forceImage;
    public float yLevelHit;

    [Space(20)]
    public LayerMask layerMask;
    public Animator animator;
    

    bool up;

    RaycastHit hit;
    Ray ray;

    void Update()
    {
        if(Input.GetMouseButton(0))
        {
            if(up)
            {
                if(force < maxForce)
                {
                    force += forceChargeSpeed * Time.deltaTime;
                } else 
                {
                    up = false;
                }
            } else
            {
                if(force > minForce)
                {
                    force -= forceChargeSpeed * Time.deltaTime;
                } else 
                {
                    up = true;
                }
            }
        }

        if(Input.GetMouseButtonUp(0))
        {
            animator.SetTrigger("Swing");
        }

        forceImage.fillAmount = Mathf.InverseLerp(minForce, maxForce, force);

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

    void OnTriggerStay(Collider other)
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Swing") && other.transform.tag == "Ball")
        {
            ball.velocity = Vector3.zero;
            Vector3 targetChange = new Vector3(target.position.x, yLevelHit, target.position.z);
            ball.AddForce(-(ball.position-targetChange) * force, ForceMode.Impulse);
        }

        if(other.tag == "Wall")
        {
            ball.velocity /= 1.5f;
        }
    }
}
