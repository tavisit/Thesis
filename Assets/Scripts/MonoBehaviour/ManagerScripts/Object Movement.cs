using System.Collections.Generic;
using UnityEngine;

public class ObjectMovement : MonoBehaviour
{
    UniversalAttractionRunner universalAttraction;
    List<OpenClBodyObject> bodyObjects;

    void Start()
    {
        universalAttraction = new UniversalAttractionRunner(".\\Assets\\Scripts\\OpenCL\\Scripts\\UniversalAttractionOpenCL.cl", "universal_attraction_force");
    }

    // Update is called once per frame
    void Update()
    {
        AccelerationComputation();
    }


    private void AccelerationComputation()
    {
        UpdateBodyObjects();
        bodyObjects = universalAttraction.Update(bodyObjects);

        foreach (var item in bodyObjects)
        {
            GameObject body = GameObject.Find(item.name);
            if (body != null)
            {
                body.GetComponent<Body>().acceleration = item.acceleration;
                Vector3 arrowDirection = item.acceleration;
                float sum = Mathf.Abs(arrowDirection.x) + Mathf.Abs(arrowDirection.y) + Mathf.Abs(arrowDirection.z);
                body.GetComponent<DrawArrow>().direction = new Vector3(arrowDirection.x / sum, arrowDirection.y / sum, arrowDirection.z / sum);
            }
        }

        bodyObjects.Clear();
    }

    public void UpdateBodyObjects(bool init=true)
    {
        GameObject[] bodies = GameObject.FindGameObjectsWithTag("Body");

        if (bodies.Length == 0) return;

        if (init)
        {
            bodyObjects = new();
            for (int i = 0; i < bodies.Length; i++)
            {
                Body values = bodies[i].GetComponent<Body>();
                bodyObjects.Add(new OpenClBodyObject(bodies[i].transform.position,
                                                     bodies[i].transform.eulerAngles,
                                                     values.mass,
                                                     bodies[i].name));
            }
        }
    }
}
