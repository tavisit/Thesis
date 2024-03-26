using UnityEngine;
using UnityEngine.UI;

public class KeyboardCameraMovement : MonoBehaviour
{
    public float normalSpeed = 5f; // Normal movement speed
    public float boostedSpeed = 50f; // Boosted movement speed when Shift is pressed
    private float currentSpeed;

    public float normalFOV = 60f; // Normal Field of View
    public float boostedFOV = 180f; // Increased Field of View when Shift is pressed
    public float backwardFOV = 20f; // Narrow FOV for backward movement
    private Camera cam;
    private float sensitivity = 5f;

    public Text cameraInfoTxt;

    public Image redBlueShiftingImage;

    private void Start()
    {
        cam = GetComponent<Camera>();
        currentSpeed = normalSpeed;
    }

    private void Update()
    {
        bool isBoosted = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        float targetFOV = normalFOV;
        Color targetColor = new Color(1, 1, 1, 0);
        if (verticalInput < 0 && isBoosted)
        {
            targetFOV = backwardFOV;
            targetColor = new Color(1, 0, 0, 0.5f);
        }
        else if (verticalInput > 0 && isBoosted)
        {
            targetFOV = boostedFOV;
            targetColor = new Color(0.9333f, 0.5098f, 0.9333f, 0.5f);
        }

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime);
        redBlueShiftingImage.color = Color.Lerp(redBlueShiftingImage.color, targetColor, Time.deltaTime);

        currentSpeed = Mathf.Lerp(currentSpeed, isBoosted ? boostedSpeed : normalSpeed, Time.deltaTime * 10);

        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * sensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * sensitivity;
            transform.eulerAngles += new Vector3(-mouseY, mouseX, 0);
        }

        Vector3 movement = (transform.forward * verticalInput + transform.right * horizontalInput).normalized;
        transform.Translate(movement * currentSpeed * Time.deltaTime, Space.World);

        cameraInfoTxt.text = $"Current Speed: {currentSpeed:F2} FOV: {cam.fieldOfView:F2}";
    }
}
