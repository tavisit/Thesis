using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    float deltaTime = 0.0f;
    bool displayFPS = true;

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            displayFPS = !displayFPS;
        }
    }

    void OnGUI()
    {
        if (displayFPS)
        {
            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;

            string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);

            int w = Screen.width, h = Screen.height;
            GUIStyle style = new GUIStyle();

            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / 100;
            style.normal.textColor = new Color(0.0f, 1.0f, 0.0f, 1.0f);

            Rect rect = new Rect(0, 0, w, h * 2 / 100);

            GUI.Label(rect, text, style);
        }
    }
}
