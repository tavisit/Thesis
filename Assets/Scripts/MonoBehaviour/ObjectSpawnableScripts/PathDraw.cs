using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PathDraw : MonoBehaviour
{
    public List<Vector3> pathPoints = new();
    public Color color = Color.white;
    private readonly float lineWidthStart = 0.1f;
    private readonly float lineWidthEnd = 0.0f;
    private LineRenderer lineRenderer;

    private void Start()
    {
        color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = lineWidthStart;
        lineRenderer.endWidth = lineWidthEnd;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        lineRenderer.textureMode = LineTextureMode.Tile;
        lineRenderer.material.mainTextureScale = new Vector2(1.0f, 1.0f);
        lineRenderer.material.mainTextureScale = new Vector2(0.1f, 1.0f);
    }

    private void Update()
    {
        if (pathPoints.Count >= 1 && !pathPoints.ToArray().Equals(lineRenderer.GetPositions(pathPoints.ToArray())))
        {
            lineRenderer.positionCount = pathPoints.Count;
            lineRenderer.SetPositions(pathPoints.ToArray());
        }
    }

}
