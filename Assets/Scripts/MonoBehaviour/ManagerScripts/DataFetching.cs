using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public static class DataFetching
{
    public static List<OpenClBodyObject> GaiaFetching(string fileName)
    {
        List<OpenClBodyObject> returnValues = new List<OpenClBodyObject>();
        TextAsset[] clFiles = Resources.LoadAll<TextAsset>("InputManagement/output");
        TextAsset mainFile = clFiles.FirstOrDefault(s => s.name.Contains(fileName));

        List<OpenClBodyObject> returnObjects = JsonHelper.FromJson<OpenClBodyObject>(mainFile.text).ToList();

        for(int i = 0; i < returnObjects.Count(); i++) 
        {
            returnObjects[i].velocity /= 5000;
        }

        return returnObjects;
    }

    public static List<OpenClBodyObject>  SolarSystemFetching()
    {
        List<OpenClBodyObject> returnValues = new List<OpenClBodyObject>();
        TextAsset[] clFiles = Resources.LoadAll<TextAsset>("InputManagement/output");
        TextAsset mainFile = clFiles.FirstOrDefault(s => s.name.Contains("solar_system"));
        return JsonHelper.FromJson<OpenClBodyObject>("{\"Items\":" + mainFile.text + "}").ToList();
    }
}
