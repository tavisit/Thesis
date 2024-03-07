using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardCameraMovement : MonoBehaviour
{
    public float speed = 15.0f;
    public float actualSpeed;
    public float sensitivity = 5.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (Input.GetKey(KeyCode.LeftShift))
        {
            actualSpeed = speed * 10;
        }
        else
        {
            actualSpeed = speed;
        }
        // Move the camera forward, backward, left, and right
        transform.position += transform.forward * Input.GetAxis("Vertical") * actualSpeed * Time.deltaTime;
        transform.position += transform.right * Input.GetAxis("Horizontal") * actualSpeed * Time.deltaTime;

        // Rotate the camera based on the mouse movement
        if (Input.GetMouseButton(1)){
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            transform.eulerAngles += new Vector3(-mouseY * sensitivity, mouseX * sensitivity, 0);
        }
    }
}
