using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Opponent : MonoBehaviour
{
    public float speed;
    public Vector2 force;
    public Vector2 yLevelHit;
    public Rigidbody ball;
    public Animator animator;
    public bool allowMove;

    float direction;

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = new Vector3(ball.position.x, 0.199f, ball.position.z);
        if(allowMove)
        transform.position = Vector3.MoveTowards(transform.position, direction, speed * Time.deltaTime);

        UpdateAnimation();
    }

    void UpdateAnimation()
	{
        Vector3 ballDir = new Vector3(ball.transform.position.x - transform.position.x, 0, ball.transform.position.z - transform.position.z);
		
        if(ballDir.z < 0) animator.SetFloat("Forward", -1);
		else animator.SetFloat("Forward", 1);

		// IF WE ARE MOVING LEFT AND RIGHT ONLY
		if(ballDir.x != 0 && Mathf.Abs(ballDir.z) <= 2)
		{
			direction = Mathf.Lerp(direction, Mathf.Clamp(ballDir.x, -1f, 1f), Time.deltaTime * 3);
		} else {
			direction = Mathf.Lerp(direction, 0, Time.deltaTime * 2);
		}

		animator.SetBool("Moving", (ballDir.x != 0 || ballDir.z != 0) && allowMove);
		animator.SetFloat("Direction", direction);

	}
    
    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Ball")
        {
            animator.SetTrigger("Swing");
            ball.velocity = Vector3.zero;
            Vector3 targetChange = new Vector3(Random.Range(-6.5f,6.2f), Random.Range(yLevelHit.x, yLevelHit.y), 20);
            ball.AddForce(-(ball.position-targetChange) * Random.Range(force.x, force.y), ForceMode.Impulse);
        }
    }
}
