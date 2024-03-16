using System.Collections.Generic;
using UnityEngine;

public class Body : MonoBehaviour
{
    public float mass;
    public Vector3 velocity;
    public Vector3 acceleration;

    protected virtual void SpecificStart() { }
    protected virtual void SpecificUpdate() { }

    protected void Start()
    {
        ApplyMaterial();
        SpecificStart();
    }

    protected void Update()
    {
        SpecificUpdate();
    }


    private void ApplyMaterial()
    {
        Material[] materials = Resources.LoadAll<Material>("MaterialsPrefabs");

        Dictionary<string, Material> materialsDict = new();
        foreach (Material mat in materials)
        {
            materialsDict.Add(mat.name, mat);
        }

        if (name.StartsWith("Star") && materialsDict.ContainsKey("Star"))
        {
            GetComponent<Renderer>().material = materialsDict["Star"];
        }
        else if (name.StartsWith("Planet") && materialsDict.ContainsKey("Planet"))
        {
            GetComponent<Renderer>().material = materialsDict["Planet"];
        }
        else if (name.StartsWith("Blackhole") && materialsDict.ContainsKey("Blackhole"))
        {
            GetComponent<Renderer>().material = materialsDict["Blackhole"];
        }
        if (materialsDict.ContainsKey("Default"))
        {
            GetComponent<Renderer>().material = materialsDict["Default"];
        }
    }
}
