using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[System.Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct OpenClBodyObject
{
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 acceleration;
    public Vector3 velocity;
    public float mass;
    public string name;

    public OpenClBodyObject(Vector3 position, Vector3 rotation, float mass, string name)
    {
        this.position = position;
        this.rotation = rotation;
        acceleration = new Vector3();
        velocity = new Vector3();
        this.mass = mass;
        this.name = name;
    }

    public List<float> Flatten()
    {
        List<float> flattenedValues = new List<float>();

        // Add Position
        flattenedValues.Add(position.x);
        flattenedValues.Add(position.y);
        flattenedValues.Add(position.z);

        // Add mass
        flattenedValues.Add(mass);

        // Add Velocity
        flattenedValues.Add(velocity.x);
        flattenedValues.Add(velocity.y);
        flattenedValues.Add(velocity.z);

        // Add Rotation
        flattenedValues.Add(rotation.x);
        flattenedValues.Add(rotation.y);
        flattenedValues.Add(rotation.z);

        // Add Acceleration
        flattenedValues.Add(acceleration.x);
        flattenedValues.Add(acceleration.y);
        flattenedValues.Add(acceleration.z);

        return flattenedValues;
    }
}