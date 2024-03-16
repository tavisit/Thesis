using UnityEngine;

public static class ViewHelper
{
    public static bool IsInView(Vector3 pointOnScreen, float farClipping)
    {
        return (pointOnScreen.z <= farClipping) && (pointOnScreen.z >= 0)
            && (pointOnScreen.x > 0) && (pointOnScreen.x < Screen.width)
            && (pointOnScreen.y > 0) && (pointOnScreen.y < Screen.height);
    }

    public static bool IsInView(Camera cam, Vector3 position)
    {
        Vector3 pointOnScreen = cam.WorldToScreenPoint(position);
        return (pointOnScreen.z <= cam.farClipPlane) && (pointOnScreen.z >= cam.nearClipPlane)
            && (pointOnScreen.x > 0) && (pointOnScreen.x < Screen.width)
            && (pointOnScreen.y > 0) && (pointOnScreen.y < Screen.height);
    }
}
