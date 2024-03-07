using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PathDraw: MonoBehaviour
{
    public List<Vector3> pathPoints = new List<Vector3>();
    public Color color = Color.white;
    private float lineWidthStart = 1;
    private float lineWidthEnd = 0.1f;
    private LineRenderer lineRenderer;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = lineWidthStart;
        lineRenderer.endWidth = lineWidthEnd;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        // Enable the use of a texture mode for the line.
        lineRenderer.textureMode = LineTextureMode.Tile;

        // Apply a simple repeating pattern to create a dotted effect.
        // Adjust the tiling to increase or decrease the density of the dots.
        lineRenderer.material.mainTextureScale = new Vector2(1.0f, 1.0f);

        // Set a small tiling value to create closer dots.
        lineRenderer.material.mainTextureScale = new Vector2(0.1f, 1.0f);
    }

    private void Update()
    {
        if (pathPoints.Count >= 1)
        {
            lineRenderer.positionCount = pathPoints.Count;
            lineRenderer.SetPositions(pathPoints.ToArray());
        }
    }

    private void ApplyFadingEffect()
    {
        Gradient gradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[2];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[5];

        // Set color keys for start and end color.
        colorKeys[0] = new GradientColorKey(lineRenderer.startColor, 0.0f);
        colorKeys[1] = new GradientColorKey(lineRenderer.endColor, 1.0f);

        // Evenly distribute alpha keys across the line to simulate fading.
        // Adjust these based on your specific needs.
        alphaKeys[0] = new GradientAlphaKey(0f, 0f); // Start fully transparent
        alphaKeys[1] = new GradientAlphaKey(0.25f, 0.125f);
        alphaKeys[2] = new GradientAlphaKey(0.5f, 0.25f);
        alphaKeys[3] = new GradientAlphaKey(0.75f, 0.375f);
        alphaKeys[4] = new GradientAlphaKey(1f, 0.5f); // Midpoint fully opaque

        gradient.SetKeys(colorKeys, alphaKeys);
        lineRenderer.colorGradient = gradient;
    }

}
