using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class OpenClBodies : ICloneable
{
    public List<OpenClBodyObject> myObjectBodies;
    public Dictionary<string, Tuple<GameObject, long>> celestialBodies;
    public float[] bounds = new float[6]; // minX, maxX, minY, maxY, minZ, maxZ


    public OpenClBodies()
    {
        myObjectBodies = new List<OpenClBodyObject>();
        celestialBodies = new Dictionary<string, Tuple<GameObject, long>>();
        myObjectBodies = DataFetching.GaiaFetching("galactic_data");

        var obj = myObjectBodies.Find(k => k.name.Equals("Blackhole Sagittarius A*"));
        float mass = obj.mass;
        Vector3 position = obj.position;

        Parallel.For(0, myObjectBodies.Count, i =>
        {
            float r = Vector3.Distance(myObjectBodies[i].position, position);
            float conversionFactor = MathF.Sqrt(Constants.G * mass / r);
            if (!float.IsNaN(conversionFactor) && float.IsFinite(conversionFactor) && conversionFactor != 0)
            {
                myObjectBodies[i].velocity = myObjectBodies[i].velocity.normalized * conversionFactor;
            }
            UpdateBounds(myObjectBodies[i]);
        });
    }

    #region ICloneable Members

    public OpenClBodies DeepClone()
    {
        return (OpenClBodies)MemberwiseClone();
    }

    #endregion

    public List<float> Flatten()
    {
        return myObjectBodies.SelectMany(obj => obj.Flatten()).ToList();
    }


    public void UpdateGraphics(Camera camera, Dictionary<string, GameObject> prefab, float pathDilation)
    {
        float fixedDelta = Time.fixedDeltaTime;

        Parallel.For(0, myObjectBodies.Count, i =>
        {
            int currentIndex = i;
            myObjectBodies[currentIndex] = UpdateEntry(pathDilation, myObjectBodies[currentIndex], fixedDelta);
            OpenClBodyObject objCl = myObjectBodies[currentIndex];
            UpdateBounds(objCl);

            UnityMainThreadDispatcher.Enqueue(() =>
            {
                if (ViewHelper.IsInView(camera, objCl.position))
                {
                    if (celestialBodies.ContainsKey(objCl.name))
                    {
                        UpdateGameObject(objCl);
                    }
                    else
                    {
                        GameObject obj = CreateGameObject(prefab, objCl);
                        celestialBodies.TryAdd(objCl.name, new Tuple<GameObject, long>(obj, CurrentMillis.GetMillis()));
                    }
                }
                else if (celestialBodies.ContainsKey(objCl.name))
                {
                    // if the object is offscreen for more than 1 minute/s, then destroy it, to free up memory
                    // aka search for stale objects
                    if (CurrentMillis.GetDifferenceSeconds(celestialBodies[objCl.name].Item2) > 60)
                    {
                        UnityEngine.Object.Destroy(celestialBodies[objCl.name].Item1);
                        celestialBodies.Remove(objCl.name);
                    }
                    else
                    {
                        celestialBodies[objCl.name].Item1.SetActive(false);
                    }
                }
            });
        });
    }

    private OpenClBodyObject UpdateEntry(float pathDilation, OpenClBodyObject entry, float fixedDelta)
    {
        if (Mathf.Abs(pathDilation) > Mathf.Epsilon)
        {
            Vector3 oldVelocity = entry.velocity;
            float deltaTime = fixedDelta * pathDilation;
            entry.velocity += entry.acceleration * deltaTime;
            entry.velocity = Vector3.ClampMagnitude(entry.velocity, Constants.MAX_VELOCITY_KPC);
            Vector3 position_at_t = oldVelocity * deltaTime + 0.5f * entry.acceleration * deltaTime * deltaTime;
            entry.position += position_at_t;
        }

        return entry;
    }

    private GameObject CreateGameObject(Dictionary<string, GameObject> prefab, OpenClBodyObject entry)
    {
        GameObject obj = ApplyBodyType(prefab, entry);
        obj.name = entry.name;

        GameObject generaicData = UnityEngine.Object.Instantiate(prefab.GetValueOrDefault("GenericData"), obj.transform.position, new Quaternion(0, 0, 0, 0), obj.transform);

        obj.GetComponentInChildren<DirectionArrowDraw>().direction = entry.acceleration;
        obj.GetComponentInChildren<PathDraw>().pathPoints = entry.pathPoints;

        float relativeRadius = entry.mass;
        obj.GetComponent<Body>().mass = entry.mass * Constants.SUN_MASS;

        if (obj.GetComponent<Blackhole>() != null)
        {
            // in case of blackhole, r  = 2*G*entry.mass/(c^c)
            relativeRadius = 2 * Constants.G_NORMAL * (entry.mass * Constants.SUN_MASS) / (Constants.MAX_VELOCITY * Constants.MAX_VELOCITY) / Constants.SUN_RADIUS;
            obj.GetComponent<Body>().velocity = new Vector3(0, 0, 0);
            obj.GetComponent<Body>().acceleration = new Vector3(0, 0, 0);
        }
        else
        {
            obj.GetComponent<Body>().velocity = entry.velocity;
            obj.GetComponent<Body>().acceleration = entry.acceleration;
        }
        obj.transform.localScale = new(relativeRadius, relativeRadius, relativeRadius);

        return obj;
    }

    private void UpdateGameObject(OpenClBodyObject objCl)
    {
        celestialBodies[objCl.name].Item1.SetActive(true);
        celestialBodies[objCl.name] =
                        new Tuple<GameObject, long>(celestialBodies[objCl.name].Item1, CurrentMillis.GetMillis());
        GameObject obj = celestialBodies[objCl.name].Item1;
        obj.GetComponent<Body>().mass = objCl.mass;
        obj.GetComponent<Body>().velocity = objCl.velocity;
        obj.GetComponent<Body>().acceleration = objCl.acceleration;
        obj.transform.position = objCl.position;
        obj.GetComponentInChildren<DirectionArrowDraw>().direction = objCl.acceleration;
        obj.GetComponentInChildren<PathDraw>().pathPoints = objCl.pathPoints;
    }

    private GameObject ApplyBodyType(Dictionary<string, GameObject> prefabs, OpenClBodyObject entry)
    {

        if (entry.name.StartsWith("Star"))
        {
            GameObject gameObject = UnityEngine.Object.Instantiate(prefabs.GetValueOrDefault("Star"), entry.position, Quaternion.Euler(new Vector3(0, 0, 0)));

            gameObject.AddComponent<StellarBody>();
            gameObject.GetComponent<StellarBody>().starTemperature = (float)entry.temperature;
            gameObject.GetComponent<StellarBody>().relativeLuminousity = (float)entry.mass;

            return gameObject;
        }
        else if (entry.name.StartsWith("Planet"))
        {
            GameObject gameObject = UnityEngine.Object.Instantiate(prefabs.GetValueOrDefault("Planet"), entry.position, Quaternion.Euler(new Vector3(0, 0, 0)));

            gameObject.AddComponent<PlanetaryBody>();

            return gameObject;
        }
        else if (entry.name.StartsWith("Blackhole"))
        {
            GameObject gameObject = UnityEngine.Object.Instantiate(prefabs.GetValueOrDefault("Blackhole"), entry.position, Quaternion.Euler(new Vector3(0, 0, 0)));

            gameObject.AddComponent<Blackhole>();

            return gameObject;
        }
        else
        {
            GameObject gameObject = UnityEngine.Object.Instantiate(prefabs.GetValueOrDefault("Star"), entry.position, Quaternion.Euler(new Vector3(0, 0, 0)));

            gameObject.AddComponent<Body>();

            return gameObject;
        }
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

    public object Clone()
    {
        throw new NotImplementedException();
    }
}
