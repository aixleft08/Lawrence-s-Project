using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Opponent : MonoBehaviour
{
    public float speed;
    public Vector2 force;
    public Vector2 yLevelHit;
    public Rigidbody ball;

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = new Vector3(ball.position.x, 0.199f, ball.position.z);
        transform.position = Vector3.MoveTowards(transform.position, direction, speed * Time.deltaTime);
    }
    
    void OnTriggerStay(Collider other)
    {
        if(other.tag == "Ball")
        {
            ball.velocity = Vector3.zero;
            Vector3 targetChange = new Vector3(Random.Range(-6.5f,6.2f), Random.Range(yLevelHit.x, yLevelHit.y), 20);
            ball.AddForce(-(ball.position-targetChange) * Random.Range(force.x, force.y), ForceMode.Impulse);
        }
    }
}
