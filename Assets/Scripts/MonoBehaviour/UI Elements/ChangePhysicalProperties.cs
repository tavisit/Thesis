using UnityEngine;
using UnityEngine.UI;

public class ChangePhysicalProperties : MonoBehaviour
{
    [SerializeField]
    private Image panelChangePhysicalProperties; 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PanelSetVisibility()
    {
        panelChangePhysicalProperties.gameObject.SetActive(!panelChangePhysicalProperties.gameObject.activeSelf);
    }
}
