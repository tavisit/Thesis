using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

[System.Serializable]
[StructLayout(LayoutKind.Sequential)]
public class OpenClBodies
{
    private readonly float relativeMass = 5.972E+12f;
    public List<OpenClBodyObject> myObjectBodies;
    public Dictionary<string, GameObject> celestialBodies;
    private readonly float maxVelocity = 299792458; // Maximum velocity

    public OpenClBodies()
    {
        myObjectBodies = new List<OpenClBodyObject>();
        celestialBodies = new Dictionary<string, GameObject>();
        ParseBodyObjects();
    }

    public List<float> Flatten()
    {
        return myObjectBodies.SelectMany(obj => obj.Flatten()).ToList();
    }

    public void UpdateGraphics(Camera camera, GameObject prefab, float pathDilation)
    {
        DestroyObjects(camera);

        Vector3[] pointsOnScreen = myObjectBodies.Select(entry => camera.WorldToScreenPoint(entry.position)).ToArray();

        for (int i = 0; i < myObjectBodies.Count; i++)
        {
            OpenClBodyObject entry = myObjectBodies[i];
            entry = UpdateEntry(pathDilation, entry);
            if (IsInView(pointsOnScreen[i], camera.farClipPlane))
            {
                if (!celestialBodies.ContainsKey(entry.name))
                {
                    GameObject obj = CreateGameObject(prefab, entry);
                    celestialBodies.Add(entry.name, obj);
                }
                else
                {
                    if (Mathf.Abs(pathDilation) > Mathf.Epsilon)
                    {
                        GameObject obj = celestialBodies[entry.name];
                        obj.GetComponent<Body>().mass = entry.mass;
                        obj.GetComponent<Body>().velocity = entry.velocity;
                        obj.GetComponent<Body>().acceleration = entry.acceleration;
                        obj.transform.position = entry.position;
                        obj.GetComponentInChildren<DirectionArrowDraw>().direction = entry.acceleration;
                        obj.GetComponentInChildren<PathDraw>().pathPoints = entry.pathPoints;
                    }
                }
            }
            myObjectBodies[i] = entry;
        }
    }

    private OpenClBodyObject UpdateEntry(float pathDilation, OpenClBodyObject entry)
    {
        if (Mathf.Abs(pathDilation) > Mathf.Epsilon)
        {
            entry.oldVelocity = entry.velocity;
            float deltaTime = Time.fixedDeltaTime * pathDilation;
            entry.velocity += entry.acceleration * deltaTime;
            entry.velocity = Vector3.ClampMagnitude(entry.velocity, maxVelocity);
            entry.position += entry.oldVelocity * deltaTime + 0.5f * entry.acceleration * deltaTime * deltaTime;
        }

        return entry;
    }

    private bool IsInView(Vector3 pointOnScreen, float farClipping)
    {
        return (pointOnScreen.z <= farClipping) && (pointOnScreen.z >= 0)
            && (pointOnScreen.x > 0) && (pointOnScreen.x < Screen.width)
            && (pointOnScreen.y > 0) && (pointOnScreen.y < Screen.height);
    }

    private void DestroyObjects(Camera camera)
    {
        List<string> objectsToRemove = new List<string>();

        foreach (var kvp in celestialBodies)
        {
            GameObject entry = kvp.Value;
            Vector3 pointOnScreen = camera.WorldToScreenPoint(entry.transform.position);
            if (!IsInView(pointOnScreen, camera.farClipPlane))
            {
                Object.Destroy(entry);
                objectsToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in objectsToRemove)
        {
            celestialBodies.Remove(key);
        }
    }

    private GameObject CreateGameObject(GameObject prefab, OpenClBodyObject entry)
    {
        GameObject obj = Object.Instantiate(prefab, entry.position, Quaternion.Euler(new Vector3(0, 0, 0)));
        obj.name = entry.name;

        GameObject accArrow = new("AccelerationArrow");
        accArrow.AddComponent<DirectionArrowDraw>();
        accArrow.transform.SetParent(obj.transform);
        accArrow.GetComponent<DirectionArrowDraw>().direction = entry.acceleration;

        GameObject pathArrow = new("PathArrow");
        pathArrow.AddComponent<PathDraw>();
        pathArrow.transform.SetParent(obj.transform);
        pathArrow.GetComponent<PathDraw>().pathPoints = entry.pathPoints;

        obj = ApplyBodyType(obj);

        obj.GetComponent<Body>().mass = entry.mass;
        obj.GetComponent<Body>().velocity = entry.velocity;
        obj.GetComponent<Body>().acceleration = entry.acceleration;

        float relativeVolume = entry.mass / relativeMass;
        obj.transform.localScale = new(relativeVolume, relativeVolume, relativeVolume);

        return obj;
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

    private void ParseBodyObjects()
    {
        myObjectBodies.Clear();
        for (int i = 0; i < 10000; i++)
        {
            myObjectBodies.Add(new OpenClBodyObject("Planet - Earth" + i.ToString(), relativeMass, new Vector3(i * 10, 0, 0), new Vector3(0, 0, 9.9823f), new Vector3(0, 0, 0)));

        }
    }
}
