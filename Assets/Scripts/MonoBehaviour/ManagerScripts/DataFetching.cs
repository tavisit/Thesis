using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DataFetching
{
    public static List<OpenClBodyObject> GaiaFetching(string fileName)
    {
        List<OpenClBodyObject> returnValues = new List<OpenClBodyObject>();
        TextAsset[] clFiles = Resources.LoadAll<TextAsset>("InputManagement");
        TextAsset mainFile = clFiles.FirstOrDefault(s => s.name.Contains(fileName));

        List<OpenClBodyObject> returnObjects = JsonHelper.FromJson<OpenClBodyObject>(mainFile.text).ToList();

        // Add Sagittarius A*
        // Actual mass in solar masses - 1.0179712045256999e+11f
        // Observed mass in solar masses - 4.297E+6f
        OpenClBodyObject sagittariusA = new OpenClBodyObject("Blackhole Sagittarius A*", 1.0179712045256999e+11f, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0));
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
