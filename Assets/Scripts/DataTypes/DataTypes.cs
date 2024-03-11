using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[System.Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct OpenClBodyObject
{
    public string name;
    public float mass;
    public Vector3 position;
    public Vector3 velocity;
    public Vector3 acceleration;
    internal Vector3 oldVelocity;

    public List<Vector3> pathPoints;

    public OpenClBodyObject(string name, float mass, Vector3 position, Vector3 velocity, Vector3 acceleration)
    {
        this.name = name;
        this.mass = mass;
        this.position = position;
        this.velocity = velocity;
        this.acceleration = acceleration;

        this.oldVelocity = this.velocity;

        pathPoints = new List<Vector3>(15);
    }
    public List<float> Flatten()
    {
        return new List<float>()
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
    }
}