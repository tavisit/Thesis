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
        List<OpenClBodyObject> movableObjects = ParseBodyObjects();
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

        foreach(OpenClBodyObject entry in movableObjects)
        {
            GameObject obj = Instantiate(prefab, entry.position, Quaternion.Euler(new Vector3(0,0,0)));
            obj.name = entry.name;

            GameObject accArrow = new("AccelerationArrow");
            accArrow.AddComponent<DirectionArrowDraw>();
            accArrow.transform.SetParent(obj.transform);

            GameObject pathArrow = new("PathArrow"); 
            pathArrow.AddComponent<PathDraw>();
            pathArrow.transform.SetParent(obj.transform);

            obj = ApplyBodyType(obj);

            float relativeVolume = entry.mass / relativeMass;
            obj.transform.localScale = new(relativeVolume, relativeVolume, relativeVolume);

            obj.GetComponent<Body>().mass = entry.mass;
            obj.GetComponent<Body>().velocity = entry.velocity;

            obj.GetComponentInChildren<PathDraw>().color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
            obj.GetComponentInChildren<DirectionArrowDraw>().color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);

            obj.GetComponent<Renderer>().material = ApplyMaterial(obj);
        }
    }

    private Material ApplyMaterial(GameObject gameObject)
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

    private GameObject ApplyBodyType(GameObject gameObject)
    {
        string objectName = gameObject.name;
        if (objectName.StartsWith("Star"))
        {
            gameObject.AddComponent<StellarBody>();
        }
        else if (objectName.StartsWith("Planet"))
        {
            gameObject.AddComponent<PlanetaryBody>();
        }
        else
        {
            gameObject.AddComponent<Body>();
        }
        return gameObject;
    }

    private List<OpenClBodyObject> ParseBodyObjects()
    {
        List<OpenClBodyObject> returnList = new();
        for(int i = 0; i < 1000; i++)
        {
            returnList.Add(new OpenClBodyObject(new Vector3(i*100, 0, 0), new Vector3(0,0, 9.9823f), relativeMass, "Planet - Earth" + i.ToString()));

        }
        returnList.Add(new OpenClBodyObject(new Vector3(-300, 0, 0), new Vector3(0, 0, 7.5459468401f), relativeMass * 10, "Planet - Jupiter"));
        returnList.Add(new OpenClBodyObject(new Vector3(400, 0, 0), new Vector3(0, 0, 0), relativeMass * 100, "Star - Sun"));
        return returnList;
    }
}
