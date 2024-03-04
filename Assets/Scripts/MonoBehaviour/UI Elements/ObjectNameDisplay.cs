using TMPro;
using UnityEngine;

public class ObjectNameDisplay : MonoBehaviour
{
    public float baseFontSize = 5; // Base font size at a reference distance
    public float referenceDistance = 10; // Reference distance at which the base font size applies
    public float maxFontSize = 20;
    private TextMeshPro nameText;

    void Start()
    {
        nameText = gameObject.GetComponent<TextMeshPro>();
        if (nameText != null)
        {
            nameText.text = gameObject.transform.parent.name;
        }
    }

    void Update()
    {
        if (Camera.main != null && nameText != null)
        {
            // Make the text always face the camera
            transform.LookAt(Camera.main.transform);
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);

            // Adjust font size based on distance to camera, with an upper limit
            float distanceToCamera = Vector3.Distance(transform.position, Camera.main.transform.position);
            float distanceRatio = distanceToCamera / referenceDistance;
            float calculatedFontSize = baseFontSize * distanceRatio;

            // Ensure the font size does not exceed the maximum limit
            nameText.fontSize = Mathf.Min(calculatedFontSize, maxFontSize);
        }
    }
}
