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
    public float yLevelHit;
    public bool canHit;

    [Space(20)]
    public LayerMask layerMask;
    public Animator animator;
    

    bool up;

    RaycastHit hit;
    Ray ray;

    public static BallHit Instance;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if(Input.GetMouseButtonUp(0))
        {
            animator.SetTrigger("Swing");
        }

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

        if(ball.velocity.z < -15f) ball.velocity = new Vector3(ball.velocity.x, ball.velocity.y, -15f);
        //if(ball.velocity.z < 0 && ball.velocity.z > -10) ball.velocity = new Vector3(ball.velocity.x, ball.velocity.y, -10f);
    }

    void OnTriggerStay(Collider other)
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Swing") && other.transform.tag == "Ball" && canHit && Gameplay.Instance.currentlyHitting == Gameplay.Turns.PLAYER)
        {
            Gameplay.Instance.gameState = Gameplay.GameState.RALLY;
            Gameplay.Instance.currentlyHitting = Gameplay.Turns.OPPONENT;

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
