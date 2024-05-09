using UnityEngine;
using UnityEngine.UI;

public class BasicStarInformation : MonoBehaviour
{
    public Text informationTextUI;
    public Image panel;
    public Image moreInformationPanel;
    public GameObject moreInforamtionButton;

    public float offset = 10;

    float clicked = 0;
    float clicktime = 0;
    readonly float clickdelay = 0.5f;

    GameObject hitObject = null;
    Body body = null;

    public GameObject starData;


    // Start is called before the first frame update
    void Start()
    {
        panel.gameObject.SetActive(false);
        moreInformationPanel.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !ViewHelper.MotionPanelIsOn())
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
                if (hitInfo.distance > 0)
                {
                    panel.gameObject.SetActive(false);
                    informationTextUI.text = "";
                    body = null;
                }
            }
        }
        else if (body != null)
        {
            string informationText = "";
            informationText += "\nName: " + hitObject.name;
            informationText += "\nPosition[kiloparsecs]:";
            informationText += "\nX: " + hitObject.transform.position.x.ToString();
            informationText += "\nY: " + hitObject.transform.position.y.ToString();
            informationText += "\nZ: " + hitObject.transform.position.z.ToString();
            informationText += "\nVelocity[kiloparsecs/Million years]: " + string.Format("{0:F2}", body.velocity.magnitude);
            informationText += "\nAcceleration[kiloparsecs/Million years^2]: " + string.Format("{0:F2}", body.acceleration.magnitude);
            informationText += "\nMass[kg]: " + (body.mass * Constants.SUN_MASS).ToString("E3");
            if (informationTextUI != null)
            {
                moreInforamtionButton.SetActive(hitObject.name.StartsWith("Star"));
                panel.gameObject.SetActive(true);
                informationTextUI.text = informationText;
            }
        }

        if (DoubleClick() && !ViewHelper.MotionPanelIsOn())
        {
            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo);
            if (hit)
            {
                Vector3 directionToCamera = (Camera.main.transform.position - hitInfo.point).normalized;
                Vector3 newCameraPosition = hitInfo.point + directionToCamera * ViewHelper.CalculateOffset(hitInfo.transform.localScale.magnitude);

                Camera.main.transform.position = newCameraPosition;
                Camera.main.transform.LookAt(hitInfo.transform.position);

            }
        }
    }
    private bool DoubleClick()
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

    public void OnClickMoreInformation()
    {
        DetailedStarInformation detailedStarInformation = new(starData, hitObject);

        detailedStarInformation.drawMap();
        moreInformationPanel.gameObject.SetActive(true);
    }

    public void CloseMoreInformation()
    {
        moreInformationPanel.gameObject.SetActive(false);
    }

    public void CloseBasicInformation()
    {
        body = null;
        panel.gameObject.SetActive(false);
    }
}
