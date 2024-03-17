using UnityEngine;

public class MapRenderer : MonoBehaviour
{
    public RenderTexture mapTexture;
    public RenderTexture frustrumTexture;
    public ObjectManager objectManager;
    public Material drawMaterial; // A material with a simple shader to draw points
    private Camera cam;

    int interval = 1;
    Vector3 minBounds;
    Vector3 maxBounds;

    void Start()
    {
        cam = Camera.main;

        minBounds = new Vector3();
        maxBounds = new Vector3();
    }

    void LateUpdate()
    {
        Vector3 minBoundsTemp = new Vector3(objectManager.openClBodies.bounds[0], objectManager.openClBodies.bounds[2], objectManager.openClBodies.bounds[4]);
        Vector3 maxBoundsTemp = new Vector3(objectManager.openClBodies.bounds[1], objectManager.openClBodies.bounds[3], objectManager.openClBodies.bounds[5]);

        if (minBoundsTemp != minBounds || maxBoundsTemp != maxBounds)
        {
            minBounds = minBoundsTemp;
            maxBounds = maxBoundsTemp;
            RenderMap();
        }

        if (Time.time % interval == 0)
        {
            RenderMap();
        }
        RenderCameraFrustrum();
    }
    public void RenderCameraFrustrum()
    {
        RenderTexture.active = frustrumTexture;

        GL.Clear(true, true, Color.clear);

        Graphics.SetRenderTarget(frustrumTexture);
        Vector3[] frustumCorners = new Vector3[4];
        cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), cam.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);
        for (int i = 0; i < frustumCorners.Length; i++)
        {
            frustumCorners[i] = cam.transform.TransformVector(frustumCorners[i]) + cam.transform.position;
        }

        Vector2[] normalizedFrustumCorners = new Vector2[frustumCorners.Length];
        for (int i = 0; i < frustumCorners.Length; i++)
        {
            normalizedFrustumCorners[i] = NormalizeToMinimap(frustumCorners[i], minBounds, maxBounds);
        }

        GL.PushMatrix();
        GL.LoadPixelMatrix(0, frustrumTexture.width, frustrumTexture.height, 0);

        GL.Begin(GL.LINES);
        drawMaterial.SetColor("_Color", Color.magenta);
        drawMaterial.SetPass(0);

        // Connect the frustum corners
        for (int i = 0; i < 4; i++)
        {
            Vector2 start = normalizedFrustumCorners[i];
            Vector2 end = normalizedFrustumCorners[(i + 1) % 4];
            GL.Vertex3(start.x * frustrumTexture.width, start.y * frustrumTexture.height, 0);
            GL.Vertex3(end.x * frustrumTexture.width, end.y * frustrumTexture.height, 0);
        }

        Vector2 camPosOnMap = NormalizeToMinimap(cam.transform.position, minBounds, maxBounds);
        for (int i = 0; i < 4; i++)
        {
            Vector2 start = normalizedFrustumCorners[i];
            GL.Vertex3(start.x * frustrumTexture.width, start.y * frustrumTexture.height, 0);
            GL.Vertex3(camPosOnMap.x * frustrumTexture.width, camPosOnMap.y * frustrumTexture.height, 0);
        }

        GL.End();
        GL.PopMatrix();
        RenderTexture.active = null;
    }

    public void RenderMap()
    {
        RenderTexture.active = mapTexture;

        GL.Clear(true, true, Color.black); 
        drawMaterial.SetColor("_Color", Color.white);

        Graphics.SetRenderTarget(mapTexture);
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, mapTexture.width, mapTexture.height, 0);
        foreach (OpenClBodyObject body in objectManager.openClBodies.myObjectBodies)
        {
            Vector2 normalizedPosition = NormalizeToMinimap(body.position, minBounds, maxBounds);

            Rect rect = new Rect(
                normalizedPosition.x * mapTexture.width,
                normalizedPosition.y * mapTexture.height,
                1,
                1
            );

            Graphics.DrawTexture(rect, Texture2D.whiteTexture, drawMaterial);
        }
        GL.PopMatrix();
        RenderTexture.active = null;
    }
    private Vector2 NormalizeToMinimap(Vector3 worldPos, Vector3 minBounds, Vector3 maxBounds)
    {
        return new Vector2(
            (worldPos.x - minBounds.x) / (maxBounds.x - minBounds.x),
            (worldPos.z - minBounds.z) / (maxBounds.z - minBounds.z)
        );
    }
}
