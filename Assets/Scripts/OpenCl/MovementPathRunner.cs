using Silk.NET.OpenCL;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovementPathRunner : OpenClRunner<Vector4>
{
    public MovementPathRunner(string filePath, string functionName) : base(filePath, functionName)
    {
    }



    public List<OpenClBodyObject> Update(List<OpenClBodyObject> args, int steps)
    {
        nint[] memObjects = new nint[2];
        int[] intObjects = new int[3] { args.Count, args[0].Flatten().Count, steps};
        Vector4[] result = new Vector4[(nuint)(args.Count * steps)];

        nuint[] globalWorkSize = new nuint[1] { (nuint)args.Count };
        nuint[] localWorkSize = new nuint[1] { 1 };

        if (OpenClInterfaceImplementation.CreateMemObjects(cl, context, memObjects, true, 0, MemFlags.ReadWrite, result))
        {
            if (OpenClInterfaceImplementation.CreateMemObjects(cl, context, memObjects, false, 1, MemFlags.ReadOnly | MemFlags.CopyHostPtr, args.SelectMany(obj => obj.Flatten()).ToArray()))
            {
                if (OpenClInterfaceImplementation.SetKernelArgsMemory(cl, kernel, memObjects, new int[] { 0, 1 }) &&
                    OpenClInterfaceImplementation.SetKernelArgsVariables(cl, kernel, intObjects, new int[] { 2, 3, 4 }))
                {
                    if (Run(globalWorkSize, localWorkSize, result.Length, memObjects, out result))
                    {
                        return args.Select((obj, index) => {
                            var movementPathArray = new Vector4[steps];
                            Array.Copy(result, index * steps, movementPathArray, 0, steps);
                            obj.movementPath = movementPathArray;
                            return obj;
                        }).ToList();
                    }
                }
            }
        }

        return args;
    }
}
