using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OpenClBodies
{
    private readonly float sunMass = 1.989E+30f; // in kgs
    private readonly float sunRadius = 696340000; // in meters
    public List<OpenClBodyObject> myObjectBodies;
    public Dictionary<string, GameObject> celestialBodies;
    private readonly float maxVelocity = 299792458; // Maximum velocity, C, in m/s

    public float[] bounds = new float[6]; // minX, maxX, minY, maxY, minZ, maxZ


    public OpenClBodies()
    {
        myObjectBodies = new List<OpenClBodyObject>();
        celestialBodies = new Dictionary<string, GameObject>();
        myObjectBodies = DataFetching.GaiaFetching("galactic_data");
        for (int i = 0; i < myObjectBodies.Count; i++)
        {
            UpdateBounds(myObjectBodies[i]);
        }
    }

    public List<float> Flatten()
    {
        return myObjectBodies.SelectMany(obj => obj.Flatten()).ToList();
    }

    public void UpdateGraphics(Camera camera, GameObject prefab, float pathDilation)
    {
        DestroyObjects(camera);

        for (int i = 0; i < myObjectBodies.Count; i++)
        {
            UpdateBounds(myObjectBodies[i]);
            OpenClBodyObject entry = myObjectBodies[i];
            entry = UpdateEntry(pathDilation, entry);
            if (ViewHelper.IsInView(camera, entry.position))
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
            Vector3 oldVelocity = entry.velocity;
            float deltaTime = Time.fixedDeltaTime * pathDilation;
            entry.velocity += entry.acceleration * deltaTime;
            entry.velocity = Vector3.ClampMagnitude(entry.velocity, maxVelocity);
            entry.position += oldVelocity * deltaTime + 0.5f * entry.acceleration * deltaTime * deltaTime;
        }

        return entry;
    }

    private void DestroyObjects(Camera camera)
    {
        List<string> objectsToRemove = new List<string>();

        foreach (var kvp in celestialBodies)
        {
            GameObject entry = kvp.Value;
            if (!ViewHelper.IsInView(camera, entry.transform.position))
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

        obj = ApplyBodyType(obj, entry.color, entry.mass);


        float relativeRadius = entry.mass;
        if (obj.GetComponent<Blackhole>() != null)
        {
            // in case of blackhole, r  = 2*G*entry.mass/(c^c)
            float G = 6.67430e-11f;
            relativeRadius = 2 * G * (entry.mass * sunMass) / (maxVelocity * maxVelocity) / sunRadius;
            obj.GetComponent<Body>().mass = entry.mass * sunMass;
            obj.GetComponent<Body>().velocity = new Vector3(0, 0, 0);
            obj.GetComponent<Body>().acceleration = new Vector3(0, 0, 0);
            obj.transform.localScale = new(relativeRadius, relativeRadius, relativeRadius);
        }
        else
        {
            obj.GetComponent<Body>().mass = entry.mass * sunMass;
            obj.GetComponent<Body>().velocity = entry.velocity;
            obj.GetComponent<Body>().acceleration = entry.acceleration;
            obj.transform.localScale = new(relativeRadius, relativeRadius, relativeRadius);
        }

        return obj;
    }

    private GameObject ApplyBodyType(GameObject gameObject, params object[] additionalParameters)
    {
        string objectName = gameObject.name;
        if (objectName.StartsWith("Star"))
        {
            gameObject.AddComponent<StellarBody>();
            gameObject.GetComponent<StellarBody>().starColor = (Color)additionalParameters[0];
            gameObject.GetComponent<StellarBody>().relativeLuminousity = (float)additionalParameters[1];
        }
        else if (objectName.StartsWith("Planet"))
        {
            gameObject.AddComponent<PlanetaryBody>();
        }
        else if (objectName.StartsWith("Blackhole"))
        {
            gameObject.AddComponent<Blackhole>();
        }
        else
        {
            gameObject.AddComponent<Body>();
        }
        return gameObject;
    }

    private void UpdateBounds(OpenClBodyObject openClBodyObject)
    {
        bounds[0] = openClBodyObject.position.x < bounds[0] ? openClBodyObject.position.x : bounds[0];
        bounds[1] = openClBodyObject.position.x > bounds[1] ? openClBodyObject.position.x : bounds[1];
        bounds[2] = openClBodyObject.position.y < bounds[2] ? openClBodyObject.position.y : bounds[2];
        bounds[3] = openClBodyObject.position.y > bounds[3] ? openClBodyObject.position.y : bounds[3];
        bounds[4] = openClBodyObject.position.z < bounds[4] ? openClBodyObject.position.z : bounds[4];
        bounds[5] = openClBodyObject.position.z > bounds[5] ? openClBodyObject.position.z : bounds[5];
    }
}
