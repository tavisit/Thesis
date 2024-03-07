using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[System.Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct OpenClBodyObject
{
    public Vector3 position;
    public Vector3 acceleration;
    public Vector3 velocity;
    public float mass;
    public string name;

    public Vector4[] movementPath;

    public OpenClBodyObject(Vector3 position, float mass, string name)
    {
        this.position = position;
        acceleration = new Vector3();
        velocity = new Vector3();
        this.mass = mass;
        this.name = name;
        movementPath = new Vector4[0];
    }

    public OpenClBodyObject(Vector3 position, Vector3 velocity, float mass, string name)
    {
        this.position = position;
        acceleration = new Vector3();
        this.velocity = velocity;
        this.mass = mass;
        this.name = name;
        movementPath = new Vector4[0];
    }
    public OpenClBodyObject(Vector3 position, Vector3 velocity, Vector3 acceleration, float mass, string name)
    {
        this.position = position;
        this.acceleration = acceleration;
        this.velocity = velocity;
        this.mass = mass;
        this.name = name;
        movementPath = new Vector4[0];
    }

    public List<float> Flatten()
    {
        List<float> flattenedValues = new()
        {
            // Add Position
            position.x,
            position.y,
            position.z,

            // Add mass
            mass,

            // Add Velocity
            velocity.x,
            velocity.y,
            velocity.z,

            // Add Acceleration
            acceleration.x,
            acceleration.y,
            acceleration.z
        };

        return flattenedValues;
    }
}