using UnityEngine;
using UnityEngine.UI;

public class ObjectInformation : MonoBehaviour
{
    public Text informationTextUI;
    public Image panel;
    public float offset = 10;

    float clicked = 0;
    float clicktime = 0;
    readonly float clickdelay = 0.5f;

    GameObject hitObject = null;
    Body body = null;


    // Start is called before the first frame update
    void Start()
    {
        panel.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

            _ = new RaycastHit();
            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo);
            if (hit)
            {
                hitObject = hitInfo.transform.gameObject;
                body = hitObject.GetComponent<Body>();
            }
            else
            {
                panel.gameObject.SetActive(false);
                informationTextUI.text = "";
                body = null;
            }
        }
        else if (body != null)
        {
            string informationText = "Information:";
            informationText += "\nName: " + hitObject.name;
            informationText += "\nPosition: " + hitObject.transform.position.ToString("E5");
            informationText += "\nVelocity: " + body.velocity.ToString("E5");
            informationText += "\nAcceleration: " + body.acceleration.ToString("E5");
            informationText += "\nMass: " + body.mass.ToString("E5");
            if (informationTextUI != null)
            {
                panel.gameObject.SetActive(true);
                informationTextUI.text = informationText;
            }
    }

        if (DoubleClick())
        {
            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo);
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
