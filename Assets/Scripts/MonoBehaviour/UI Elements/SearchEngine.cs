using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class SearchEngine : MonoBehaviour
{
    [SerializeField] private TMP_InputField searchField;
    [SerializeField] private ScrollRect scrollView;
    [SerializeField] private ObjectManager objectManager;

    private TextMeshProUGUI scrollViewText;

    private string searchFieldText;

    void Start()
    {
        searchFieldText = "";
        scrollViewText = GetAllChildren(scrollView.gameObject).FirstOrDefault(obj => obj.Key.Equals("SearchResultsTextBox")).Value.GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (searchFieldText.Length == 0)
        {
            scrollView.gameObject.SetActive(false);
        }
        else
        {
            scrollView.gameObject.SetActive(true);

        }

    }
    Dictionary<string, GameObject> GetAllChildren(GameObject obj)
    {
        Dictionary<string, GameObject> children = new();

        foreach (Transform child in obj.transform)
        {
            children.Add(child.gameObject.name, child.gameObject);
            children.AddRange(GetAllChildren(child.gameObject));
        }

        return children;
    }


    public void OnInputValueChanged()
    {
        if (searchField.text.Length <= 1)
        {
            searchFieldText = "";
            return;
        }

        searchFieldText = searchField.text.ToLower();

        List<OpenClBodyObject> objects = objectManager.openClBodies.myObjectBodies
                                                                                .AsParallel()
                                                                                .Where(obj => obj.name.ToLower().Contains(searchFieldText))
                                                                                .ToList();

        StringBuilder sb = new StringBuilder();
        sb.AppendLine();
        foreach (OpenClBodyObject obj in objects)
        {
            sb.AppendLine("- <link=\"" + obj.name + "\">" + obj.name + "</link>");
            sb.AppendLine();
        }
        scrollViewText.text = sb.ToString();
    }
}
