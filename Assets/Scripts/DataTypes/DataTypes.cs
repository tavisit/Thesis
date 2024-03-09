using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[System.Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct OpenClBodyObject
{
    public Vector3 position;
    public Vector3 velocity;
    public float mass;
    public string name;

    public OpenClBodyObject(Vector3 position, Vector3 velocity, float mass, string name)
    {
        this.position = position;
        this.velocity = velocity;
        this.mass = mass;
        this.name = name;
    }
}