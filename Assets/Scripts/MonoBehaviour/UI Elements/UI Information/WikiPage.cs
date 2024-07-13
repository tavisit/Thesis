using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WikiPage : MonoBehaviour
{
    private List<WikiPageObjList> wikiPageObjLists;

    [SerializeField]
    private Image panelWikiPage;

    [SerializeField]
    private GameObject buttonTemplate;

    [SerializeField]
    private GameObject categoryContent;

    [SerializeField]
    private GameObject pagesContent;

    [SerializeField]
    private TMP_Text informationTitle;

    [SerializeField]
    private TMP_Text informationBody;

    DataFetching dataFetching;

    // Start is called before the first frame update
    void Start()
    {
        dataFetching = DataFetching.Instance;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PanelSetVisibility()
    {
        ClearPage(pagesContent.transform);
        ClearPage(categoryContent.transform);
        UpdateWikiData();
        panelWikiPage.gameObject.SetActive(!panelWikiPage.gameObject.activeSelf);
    }

    private void SelectCategory(string category)
    {
        WikiPageObjList currentCategory = wikiPageObjLists.Find(x => x.Category == category);

        if (currentCategory != null)
        {
            ClearPage(pagesContent.transform);
        }
        else
        {
            return;
        }

        for (int j = 0; j < currentCategory.Pages.Count; j++)
        {
            string title = currentCategory.Pages[j].Title;
            string description = currentCategory.Pages[j].Description;

            GameObject btn2 = Instantiate(buttonTemplate);
            btn2.GetComponentInChildren<TMP_Text>().text = title;
            btn2.transform.SetParent(pagesContent.transform);
            btn2.GetComponent<Button>().onClick.AddListener(() => SelectPage(title, description));
        }
    }

    private void SelectPage(string title, string description)
    {
        informationTitle.text = title;
        informationBody.text = "\n\n" + description;
    }

    private void ClearPage(Transform t)
    {
        var children = t.Cast<Transform>().ToArray();

        foreach (var child in children)
        {
            Object.DestroyImmediate(child.gameObject);
        }

        informationTitle.text = "";
        informationBody.text = "";
    }

    private void UpdateWikiData()
    {
        wikiPageObjLists = dataFetching.WikiFetching("wiki_data.json");

        for (int i = 0; i < wikiPageObjLists.Count; i++)
        {
            string category = wikiPageObjLists[i].Category;

            GameObject btn = Instantiate(buttonTemplate);
            btn.GetComponentInChildren<TMP_Text>().text = category;
            btn.transform.SetParent(categoryContent.transform);
            btn.GetComponent<Button>().onClick.AddListener(() => SelectCategory(category));
        }
    }
}
