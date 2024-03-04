using UnityEngine;
using UnityEngine.UI;

public class ObjectInformation : MonoBehaviour
{
    public Text informationTextUI;
    public float offset = 10;

    float clicked = 0;
    float clicktime = 0;
    float clickdelay = 0.5f;

    GameObject hitObject = null;
    Body body = null;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _ = new RaycastHit();
            RaycastHit hitInfo;
            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
            if (hit)
            {
                hitObject = hitInfo.transform.gameObject;
                body = hitObject.GetComponent<Body>();
            }
        }
        else
        {
            string informationText = string.Empty;
            if (body != null)
            {
                informationText += "\nName: " + hitObject.name;
                informationText += "\nPosition: " + hitObject.transform.position.ToString("E5");
                informationText += "\nVelocity: " + body.velocity.ToString("E5");
                informationText += "\nAcceleration: " + body.acceleration.ToString("E5");
                informationText += "\nMass: " + body.mass.ToString("E5");
            }
            if(informationTextUI != null)
            {
                informationTextUI.text = informationText;
            }
        }

        if (DoubleClick())
        {
            RaycastHit hitInfo;
            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
            if (hit)
            {
                Vector3 directionToCamera = (Camera.main.transform.position - hitInfo.point).normalized;
                Vector3 newCameraPosition = hitInfo.point + directionToCamera * offset;

                Camera.main.transform.position = newCameraPosition;
                Camera.main.transform.LookAt(hitInfo.transform.position);

            }
        }
    }



    bool DoubleClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            clicked++;
            if (clicked == 1) clicktime = Time.time;
        }
        if (clicked > 1 && Time.time - clicktime < clickdelay)
        {
            clicked = 0;
            clicktime = 0;
            return true;
        }
        else if (clicked > 2 || Time.time - clicktime > 1) clicked = 0;
        return false;
    }
}
