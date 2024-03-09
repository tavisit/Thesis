using Silk.NET.OpenCL;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovementPathRunner : OpenCLRunner<Vector4, Dictionary<string, List<Vector3>>, List<GameObject>>
{
    public MovementPathRunner(string filePath, string functionName) : base(filePath, functionName)
    {
    }

    public override void Update(List<GameObject> args, params object[] additionalParameters)
    {
        int argsLength = args.Count;
        int steps = (int)additionalParameters[1];
        nint[] memObjects = new nint[2];
        int[] intObjects = new int[3] { argsLength, (int)additionalParameters[0], steps};
        Vector4[] result = new Vector4[(nuint)(argsLength * steps)];

        nuint[] globalWorkSize = new nuint[1] { (nuint)argsLength };
        nuint[] localWorkSize = new nuint[1] { 1 };

        if (OpenCLInterfaceImplementation.CreateMemObjects(cl, context, memObjects, true, 0, MemFlags.ReadWrite, result)
            && OpenCLInterfaceImplementation.CreateMemObjects(cl, context, memObjects, false, 1, MemFlags.ReadOnly | MemFlags.CopyHostPtr, 
                                            args.SelectMany(obj => {
                                                Body currentObj = obj.GetComponent<Body>();
                                                return Flatten(obj.transform.position, currentObj.mass, currentObj.velocity, currentObj.acceleration);
                                            }).ToArray())
            && OpenCLInterfaceImplementation.SetKernelArgsMemory(cl, kernel, memObjects, new int[] { 0, 1 })
            && OpenCLInterfaceImplementation.SetKernelArgsVariables(cl, kernel, intObjects, new int[] { 2, 3, 4 })
            && Run(globalWorkSize, localWorkSize, result.Length, memObjects, 0, out result))
        {
            for(int index =0;index < argsLength; index++)
            {
                args[index].GetComponentInChildren<PathDraw>().pathPoints.Clear();
                for (int step =0;step<steps; step++)
                {
                    args[index].GetComponentInChildren<PathDraw>().pathPoints.Add(result[index*steps+step]);
                }
            }
        }
    }
}
