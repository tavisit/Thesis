using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DirectionArrowDraw : MonoBehaviour

{
    public Vector3 direction;
    private Vector3 lastDirection;
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

        Vector3 startPoint = transform.position;
        Vector3 arrowEndPoint = startPoint + direction.normalized * arrowHeadLength;
        Vector3 arrowBasePoint = startPoint + (direction.normalized * (arrowHeadLength - 0.5f));

        float arrowHeadWidth = 0.5f;
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized * arrowHeadWidth;

        Vector3 arrowBasePoint1 = arrowBasePoint + perpendicular;
        Vector3 arrowBasePoint2 = arrowBasePoint - perpendicular;

        // Set LineRenderer
        lineRenderer.positionCount = 5;
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, arrowEndPoint);
        lineRenderer.SetPosition(2, arrowBasePoint1);
        lineRenderer.SetPosition(3, arrowBasePoint2);
        lineRenderer.SetPosition(4, arrowEndPoint);

        lastDirection = direction;
    }
}
