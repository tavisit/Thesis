using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateObjects : MonoBehaviour
{
    public GameObject prefab;

    private readonly float relativeMass = 5.972E+12f;

    private Dictionary<string, Material> materialsDict;

    // Start is called before the first frame update
    void Start()
    {
        List<OpenClBodyObject> movableObjects = parseBodyObjects();
        if (prefab == null || movableObjects.Count <= 0)
        {
            Debug.LogError("Setup incomplete or invalid.");
            return;
        }

        // Load all materials from the specified folder
        Material[] materials = Resources.LoadAll<Material>("MaterialsPrefabs");

        // Populate the dictionary with loaded materials
        materialsDict = new Dictionary<string, Material>();
        foreach (Material mat in materials)
        {
            materialsDict.Add(mat.name, mat);
        }

        int index = -3;
        foreach(OpenClBodyObject entry in movableObjects)
        {
            GameObject obj = Instantiate(prefab, entry.position, Quaternion.Euler(entry.rotation));
            obj.name = entry.name;

            obj.GetComponent<Body>().mass = entry.mass;

            float relativeVolume = entry.mass / relativeMass;
            obj.transform.localScale = new(relativeVolume, relativeVolume, relativeVolume);
            obj.GetComponent<DrawArrow>().color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
            obj.GetComponent<Renderer>().material = applyMaterial(obj);
            index++;
        }
    }

    private Material applyMaterial(GameObject gameObject)
    {
        string objectName = gameObject.name;
        if (objectName.StartsWith("Star") && materialsDict.ContainsKey("Star"))
        {
            return materialsDict["Star"];
        }
        else if (objectName.StartsWith("Planet") && materialsDict.ContainsKey("Planet"))
        {
            return materialsDict["Planet"];
        }

        if (materialsDict.ContainsKey("Default"))
        {
            return materialsDict["Default"];
        }

        // If no material named "Default" is found, log a warning and return null
        Debug.LogWarning("No material named 'Default' found.");
        return null;
    }

    private List<OpenClBodyObject> parseBodyObjects()
    {
        List<OpenClBodyObject> returnList = new List<OpenClBodyObject>();
        returnList.Add(new OpenClBodyObject(new Vector3(0, 0, 0), new Vector3(0, 0, 0), relativeMass, "Planet - Earth"));
        returnList.Add(new OpenClBodyObject(new Vector3(10, 10, 10), new Vector3(0, 0, 0), relativeMass * 10, "Planet - Jupiter"));
        returnList.Add(new OpenClBodyObject(new Vector3(0, 400, 400), new Vector3(0, 0, 0), relativeMass * 100, "Star - Sun"));
        return returnList;
    }
}
