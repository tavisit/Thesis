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
    private float sensitivity = 5f;

    public Text cameraInfoTxt;
    public Image redBlueShiftingImage;
    
    public Slider timeDilationSlider;
    float oldValueSlider;

    private void Start()
    {
        currentSpeed = normalSpeed;
    }

    private void Update()
    {
        bool isMoving = moveCamera();
        bool isRotating = rotateCamera();

        if (isMoving || isRotating)
        {
            if (timeDilationSlider.value != 0)
            {
                oldValueSlider = timeDilationSlider.value;
                timeDilationSlider.value = 0;
            }
        }
        else
        {
            if (timeDilationSlider.value == 0)
            {
                timeDilationSlider.value = oldValueSlider;
            }
        }

        cameraInfoTxt.text = $"Current Speed: {currentSpeed:F2} FOV: {Camera.main.fieldOfView:F2}";
    }

    private bool moveCamera()
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

        Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, targetFOV, Time.deltaTime);
        redBlueShiftingImage.color = Color.Lerp(redBlueShiftingImage.color, targetColor, Time.deltaTime);

        currentSpeed = Mathf.Lerp(currentSpeed, isBoosted ? boostedSpeed : normalSpeed, Time.deltaTime * 10);

        Vector3 movement = (transform.forward * verticalInput + transform.right * horizontalInput).normalized;
        transform.Translate(movement * currentSpeed * Time.deltaTime, Space.World);


        bool isMoved = false;
        if (Mathf.Abs(horizontalInput) > Mathf.Epsilon || Mathf.Abs(verticalInput) > Mathf.Epsilon)
        {
            isMoved = true;
        }
        return isMoved;
    }

    private bool rotateCamera()
    {
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * sensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * sensitivity;
            transform.eulerAngles += new Vector3(-mouseY, mouseX, 0);
            return true;
        }
        return false;
    }

    public void OnClickResetSlider()
    {
        if (timeDilationSlider != null)
        {
            timeDilationSlider.value = 0;
            oldValueSlider = 0;
        }
    }
}
