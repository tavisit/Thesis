using UnityEngine;

public class DrawArrow : MonoBehaviour
{
    public Vector3 direction;

    public Color color;
    public float arrowHeadLength = 2.0f;

    LineRenderer lineRenderer;

    private void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
    }
    private void Update()
    {
        Draw();
    }

    private void Draw()
    {
        direction = direction.normalized * arrowHeadLength * transform.localScale.x; // Now 'direction' also represents the length of the arrow
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f; // Keep the end width consistent for the shaft
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        // Set the positions for the line renderer to draw the arrow shaft
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position + direction);
    }
}
