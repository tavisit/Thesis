using Silk.NET.OpenCL;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UniversalAttractionRunner : OpenCLRunner<Vector4, Dictionary<string, Vector3>, List<GameObject>>
{
    public UniversalAttractionRunner(string filePath, string functionName) : base(filePath, functionName)
    {
    }

    public override void Update(List<GameObject> args, params object[] additionalParameters)
    {
        int argsLength = args.Count;
        nint[] memObjects = new nint[2];
        int[] valueObjects = new int[2] { argsLength, (int)additionalParameters[0]};
        Vector4[] result = new Vector4[(nuint)argsLength];

        nuint[] globalWorkSize = new nuint[1] { (nuint)argsLength };
        nuint[] localWorkSize = new nuint[1] { 1 };


        if (OpenCLInterfaceImplementation.CreateMemObjects(cl, context, memObjects, true, 0, MemFlags.ReadWrite, result)
            && OpenCLInterfaceImplementation.CreateMemObjects(cl, context, memObjects, false, 1, MemFlags.ReadOnly | MemFlags.CopyHostPtr,
                                            args.SelectMany(obj => {
                                                Body currentObj = obj.GetComponent<Body>();
                                                return Flatten(obj.transform.position, currentObj.mass, currentObj.velocity, currentObj.acceleration);
                                            }).ToArray())
            && OpenCLInterfaceImplementation.SetKernelArgsMemory(cl, kernel, memObjects, new int[] { 0, 1 })
            && OpenCLInterfaceImplementation.SetKernelArgsVariables(cl, kernel, valueObjects, new int[] { 2, 3 })
            && Run(globalWorkSize, localWorkSize, result.Length, memObjects, 0, out result))
        {
            for (int index = 0; index < result.Length; index++)
            {
                args[index].GetComponent<Body>().acceleration = result[index];
                args[index].GetComponentInChildren<DirectionArrowDraw>().direction = result[index];
            }
        }
    }
}