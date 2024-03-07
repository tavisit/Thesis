using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectMovement : MonoBehaviour
{
    UniversalAttractionRunner universalAttraction;
    List<OpenClBodyObject> bodyObjects;
    MovementPathRunner movementPathRunner;

    int nr_steps = 25;

    void Start()
    {
        universalAttraction = new UniversalAttractionRunner("OpenCL_ComputeAcceleration", "universal_attraction_force");
        movementPathRunner = new MovementPathRunner("OpenCL_ComputePath", "compute_movement_path");
        bodyObjects = new();
    }

    // Update is called once per frame
    void Update()
    {
        GameObject[] bodies = GameObject.FindGameObjectsWithTag("Body");

        // UPDATE THE BODIES LIST
        bodyObjects.Clear();
        for (int i = 0; i < bodies.Length; i++)
        {
            {
                Body values = bodies[i].GetComponent<Body>();
                bodyObjects.Add(new OpenClBodyObject(bodies[i].transform.position,
                                                        values.velocity,
                                                        bodies[i].transform.eulerAngles,
                                                        values.acceleration,
                                                        values.mass,
                                                        bodies[i].name));
            }
        }

        // update the acceleration
        bodyObjects = universalAttraction.Update(bodyObjects);

        // update the path
        bodyObjects = movementPathRunner.Update(bodyObjects, nr_steps);

        // update the objects
        foreach (var item in bodyObjects)
        {
            GameObject body = GameObject.Find(item.name);
            if (body != null)
            {
                body.GetComponent<Body>().acceleration = item.acceleration;

                body.GetComponentInChildren<PathDraw>().pathPoints.Clear();
                body.GetComponentInChildren<PathDraw>().pathPoints.AddRange(item.movementPath.Select(vec4 => new Vector3(vec4.x, vec4.y, vec4.z)).ToList());

                body.GetComponentInChildren<DirectionArrowDraw>().direction = item.acceleration.normalized;
            }
        }
    }
}
