using System.Collections.Generic;
using System.IO;
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
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
        if (File.Exists(filePath))
        {
            string jsonContent = File.ReadAllText(filePath);
            List<OpenClBodyObject> returnObjects = JsonHelper.FromJson<OpenClBodyObject>(jsonContent).ToList();

            FindMinMaxPositions(returnObjects);
            return returnObjects;
        }
        else
        {
            Debug.LogError("JSON file not found!");
            return new List<OpenClBodyObject>();
        }


    }

    public List<WikiPageObjList> WikiFetching(string fileName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
        if (File.Exists(filePath))
        {
            string jsonContent = File.ReadAllText(filePath);
            List<WikiPageObjList> returnObjects = JsonHelper.FromJson<WikiPageObjList>(jsonContent).ToList();
            return returnObjects;
        }
        else
        {
            Debug.LogError("JSON file not found!");
            return new List<WikiPageObjList>();
        }
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
