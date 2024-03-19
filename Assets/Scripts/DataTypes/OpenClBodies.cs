﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class OpenClBodies
{
    private readonly float sunMass = 1.989E+30f; // in kgs
    private readonly float sunRadius = 696340000; // in meters
    public List<OpenClBodyObject> myObjectBodies;
    public ConcurrentDictionary<string, GameObject> celestialBodies;
    private readonly float maxVelocity = 299792458; // Maximum velocity, C, in m/s

    public float[] bounds = new float[6]; // minX, maxX, minY, maxY, minZ, maxZ


    public OpenClBodies()
    {
        myObjectBodies = new List<OpenClBodyObject>();
        celestialBodies = new ConcurrentDictionary<string, GameObject>();
        myObjectBodies = DataFetching.GaiaFetching("galactic_data");
        Parallel.For(0, myObjectBodies.Count, i =>
        {
            int currentIndex = i;
            UpdateBounds(myObjectBodies[i]);
        });
    }

    public List<float> Flatten()
    {
        return myObjectBodies.SelectMany(obj => obj.Flatten()).ToList();
    }

    public void UpdateGraphics(Camera camera, Dictionary<string, GameObject> prefab, float pathDilation)
    {
        float fixedDelta = Time.fixedDeltaTime;
        DestroyObjects(camera);

        Parallel.For(0, myObjectBodies.Count, i =>
        {
            int currentIndex = i;
            UpdateBounds(myObjectBodies[currentIndex]);
            myObjectBodies[currentIndex] = UpdateEntry(pathDilation, myObjectBodies[currentIndex], fixedDelta);

            UnityMainThreadDispatcher.Enqueue(() =>
            {
                if (ViewHelper.IsInView(camera, myObjectBodies[currentIndex].position))
                {
                    if (!celestialBodies.ContainsKey(myObjectBodies[currentIndex].name))
                    {
                        GameObject obj = CreateGameObject(prefab, myObjectBodies[currentIndex]);
                        celestialBodies.TryAdd(myObjectBodies[currentIndex].name, obj);
                    }
                    else if (Mathf.Abs(pathDilation) > Mathf.Epsilon)
                    {
                        GameObject obj = celestialBodies[myObjectBodies[currentIndex].name];
                        obj.GetComponent<Body>().mass = myObjectBodies[currentIndex].mass;
                        obj.GetComponent<Body>().velocity = myObjectBodies[currentIndex].velocity;
                        obj.GetComponent<Body>().acceleration = myObjectBodies[currentIndex].acceleration;
                        obj.transform.position = myObjectBodies[currentIndex].position;
                        obj.GetComponentInChildren<DirectionArrowDraw>().direction = myObjectBodies[currentIndex].acceleration;
                        obj.GetComponentInChildren<PathDraw>().pathPoints = myObjectBodies[currentIndex].pathPoints;
                    }
                }
            });
        });
    }

    private OpenClBodyObject UpdateEntry(float pathDilation, OpenClBodyObject entry, float fixedDelta)
    {
        if (Mathf.Abs(pathDilation) > Mathf.Epsilon)
        {
            Vector3 oldVelocity = entry.velocity / 1E+6f;
            float deltaTime = fixedDelta * pathDilation;
            entry.velocity += entry.acceleration * deltaTime;
            entry.velocity = Vector3.ClampMagnitude(entry.velocity, maxVelocity);
            Vector3 position_at_t = oldVelocity * deltaTime + 0.5f * entry.acceleration * deltaTime * deltaTime;
            entry.position += position_at_t;
        }

        return entry;
    }

    private void DestroyObjects(Camera camera)
    {
        ConcurrentBag<string> objectsToRemove = new ConcurrentBag<string>();

        Parallel.ForEach(celestialBodies, kvp =>
        {
            GameObject entry = kvp.Value;

            try
            {
                UnityMainThreadDispatcher.Enqueue(() =>
                {
                    if (kvp.Value == null)
                    {
                        celestialBodies.TryRemove(kvp.Key, out _);
                    }
                    else if (entry != null && !ViewHelper.IsInView(camera, entry.transform.position))
                    {
                        UnityEngine.Object.Destroy(entry);
                        celestialBodies.TryRemove(kvp.Key, out _);
                    }
                });

            }
            catch (InvalidOperationException ex)
            {
                Debug.Log(ex);
                celestialBodies.TryRemove(kvp.Key, out _);
            }
        });
    }

    private GameObject CreateGameObject(Dictionary<string, GameObject> prefab, OpenClBodyObject entry)
    {
        GameObject obj = ApplyBodyType(prefab, entry);
        obj.name = entry.name;

        GameObject accArrow = new("AccelerationArrow");
        accArrow.AddComponent<DirectionArrowDraw>();
        accArrow.transform.SetParent(obj.transform);
        accArrow.GetComponent<DirectionArrowDraw>().direction = entry.acceleration;

        GameObject pathArrow = new("PathArrow");
        pathArrow.AddComponent<PathDraw>();
        pathArrow.transform.SetParent(obj.transform);
        pathArrow.GetComponent<PathDraw>().pathPoints = entry.pathPoints;




        float relativeRadius = entry.mass;
        obj.GetComponent<Body>().mass = entry.mass * sunMass;

        if (obj.GetComponent<Blackhole>() != null)
        {
            // in case of blackhole, r  = 2*G*entry.mass/(c^c)
            float G = 6.67430e-11f;
            relativeRadius = 2 * G * (entry.mass * sunMass) / (maxVelocity * maxVelocity) / sunRadius;
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
}
