using UnityEngine;
using UnityEngine.UI;

public class WikiPage : MonoBehaviour
{
    [SerializeField]
    private Image panelWikiPage;

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
        panelWikiPage.gameObject.SetActive(!panelWikiPage.gameObject.activeSelf);
    }
}
