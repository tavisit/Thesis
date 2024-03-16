using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DataFetching
{
    public static List<OpenClBodyObject> GaiaFetching(string fileName)
    {
        List<OpenClBodyObject> returnValues = new List<OpenClBodyObject>();
        TextAsset[] clFiles = Resources.LoadAll<TextAsset>("InputManagement/output");
        TextAsset mainFile = clFiles.FirstOrDefault(s => s.name.Contains(fileName));

        List<OpenClBodyObject> returnObjects = JsonHelper.FromJson<OpenClBodyObject>(mainFile.text).ToList();

        for (int i = 0; i < returnObjects.Count(); i++)
        {
            returnObjects[i].velocity /= 5000;
        }

        // Add Sagittarius A*
        OpenClBodyObject sagittariusA = new OpenClBodyObject("Blackhole Sagittarius A*", 4.297E+6f, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0));
        returnObjects.Add(sagittariusA);

        return returnObjects;
    }

    public static List<OpenClBodyObject> SolarSystemFetching()
    {
        List<OpenClBodyObject> returnValues = new List<OpenClBodyObject>();
        TextAsset[] clFiles = Resources.LoadAll<TextAsset>("InputManagement/output");
        TextAsset mainFile = clFiles.FirstOrDefault(s => s.name.Contains("solar_system"));
        return JsonHelper.FromJson<OpenClBodyObject>("{\"Items\":" + mainFile.text + "}").ToList();
    }
}
