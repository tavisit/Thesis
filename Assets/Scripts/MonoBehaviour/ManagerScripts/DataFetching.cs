using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataFetching
{
    private Vector3 minPosition;
    private Vector3 maxPosition;

    private static readonly DataFetching instance = new DataFetching();

    // Private constructor to prevent instantiation
    private DataFetching()
    {
    }

    // Public static method to access the singleton instance
    public static DataFetching Instance
    {
        get
        {
            return instance;
        }
    }

    public List<OpenClBodyObject> GaiaFetching(string fileName)
    {
        List<OpenClBodyObject> returnValues = new List<OpenClBodyObject>();
        TextAsset[] clFiles = Resources.LoadAll<TextAsset>("InputManagement");
        TextAsset mainFile = clFiles.FirstOrDefault(s => s.name.Contains(fileName));

        List<OpenClBodyObject> returnObjects = JsonHelper.FromJson<OpenClBodyObject>(mainFile.text).ToList();

        FindMinMaxPositions(returnObjects);

        // Add Sagittarius A*
        // Actual mass in solar masses - 1.0179712045256999e+11f
        // Observed mass in solar masses - 4.297E+6f
        OpenClBodyObject sagittariusA = new OpenClBodyObject("Blackhole Sagittarius A*", 1.0179712045256999e+11f, new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0));
        returnObjects.Add(sagittariusA);

        // Add random blackholes in the galaxy
        for (int i = 0; i < 20; i++)
        {
            float x = Random.Range(minPosition.x, maxPosition.x);
            float y = Random.Range(minPosition.y, maxPosition.y);
            float z = Random.Range(minPosition.z, maxPosition.z);

            OpenClBodyObject blackHole = new OpenClBodyObject("Blackhole " + Random.Range(0, 9999).ToString(), Random.Range(3.0f, 10.0f), new Vector3(x, y, z), new Vector3(0, 0, 0), new Vector3(0, 0, 0));
            returnObjects.Add(blackHole);
        }

        return returnObjects;
    }

    public List<OpenClBodyObject> SolarSystemFetching()
    {
        List<OpenClBodyObject> returnValues = new List<OpenClBodyObject>();
        TextAsset[] clFiles = Resources.LoadAll<TextAsset>("InputManagement/output");
        TextAsset mainFile = clFiles.FirstOrDefault(s => s.name.Contains("solar_system"));
        return JsonHelper.FromJson<OpenClBodyObject>("{\"Items\":" + mainFile.text + "}").ToList();
    }

    private void FindMinMaxPositions(List<OpenClBodyObject> returnObjects)
    {
        minPosition = returnObjects[0].position;
        maxPosition = returnObjects[0].position;

        foreach (var obj in returnObjects)
        {
            // Update min and max positions
            minPosition = Vector3.Min(minPosition, obj.position);
            maxPosition = Vector3.Max(maxPosition, obj.position);
        }
    }
}
