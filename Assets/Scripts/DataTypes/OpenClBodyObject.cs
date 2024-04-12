using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[Serializable]
[StructLayout(LayoutKind.Sequential)]
public class OpenClBodyObject : ICloneable
{
    public string name;
    public float mass;
    public Vector3 position;
    public Vector3 velocity;
    public float temperature;
    [NonSerialized]
    public Vector3 acceleration = new();

    [NonSerialized]
    public List<Vector3> pathPoints = new(15);

    public OpenClBodyObject(string name, float mass, Vector3 position, Vector3 velocity, Vector3 acceleration)
    {
        this.name = name;
        this.mass = mass;
        this.position = position;
        this.velocity = velocity;
        this.acceleration = acceleration;
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

    #region ICloneable Members

    public OpenClBodyObject DeepClone()
    {
        return (OpenClBodyObject)MemberwiseClone();
    }

    public object Clone()
    {
        return MemberwiseClone();
    }

    #endregion
}