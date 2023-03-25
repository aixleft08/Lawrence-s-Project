using UnityEngine;

public class BallBouncePrediction : MonoBehaviour
{
    public int numSegments = 20; // Number of line segments to draw
    public float segmentSpacing = 0.1f; // Distance between line segments
    public Color lineColor = Color.white; // Color of the line

    private LineRenderer lineRenderer;
    private Vector3[] segments;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = numSegments;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;

        segments = new Vector3[numSegments];

        Gameplay.Instance.AllowPlayerMovement(false);
    }

    void Update()
    {
        // Calculate the trajectory of the ball
        Vector3 ballVelocity = GetComponent<Rigidbody>().velocity;
        Vector3 ballPosition = transform.position;

        for (int i = 0; i < numSegments; i++)
        {
            float time = segmentSpacing * i;
            segments[i] = ballPosition + ballVelocity * time + 0.5f * Physics.gravity * time * time;
        }

        // Update the line renderer with the trajectory points
        lineRenderer.SetPositions(segments);

        if(segments.Length <= 0) return;
        Vector3 direction = new Vector3(segments[0].x, 0.199f, segments[0].z);

        if(Gameplay.Instance.currentlyHitting == Gameplay.Turns.PLAYER)
        FirstPersonController.Instance.transform.position = Vector3.MoveTowards(FirstPersonController.Instance.transform.position, direction, FirstPersonController.Instance.walkSpeed * Time.deltaTime);
    }
}
