using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DirectionArrowDraw : MonoBehaviour

{
    public Vector3 direction;
    public Vector3 lastDirection;
    public Color color = Color.white;
    private readonly float arrowHeadLength = 2.0f;
    private readonly float lineWidth = 0.1f;
    private LineRenderer lineRenderer;

    private void Start()
    {
        color = Color.white;
        lineRenderer = GetComponent<LineRenderer>();

        // These settings may duplicate those in DrawPath if they share the same LineRenderer
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }

    private void Update()
    {
        if (direction != Vector3.zero)
        {
            UpdateArrow(direction);
        }
        else if(lastDirection != Vector3.zero)
        {
            UpdateArrow(lastDirection);
        }
    }

    private void UpdateArrow(Vector3 direction)
    {
        transform.position = transform.parent.position;

        Vector3 lastPoint = transform.position;
        Vector3 arrowEndPoint = lastPoint + direction.normalized * arrowHeadLength;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, lastPoint);
        lineRenderer.SetPosition(1, arrowEndPoint);
        lastDirection = direction;
    }
}
