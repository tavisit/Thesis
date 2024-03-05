using Silk.NET.OpenCL;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class UniversalAttractionRunner : OpenClRunner<Vector4>
{
    public UniversalAttractionRunner(string filePath, string functionName) : base(filePath, functionName)
    {
    }

    public List<OpenClBodyObject> Update(List<OpenClBodyObject> args)
    {
        nint[] memObjects = new nint[2];
        int[] valueObjects = new int[2] { args.Count, args[0].Flatten().Count};
        Vector4[] result = new Vector4[(nuint)args.Count];

        nuint[] globalWorkSize = new nuint[1] { (nuint)args.Count };
        nuint[] localWorkSize = new nuint[1] { 1 };

        if (OpenClInterfaceImplementation.CreateMemObjects(cl, context, memObjects, true, 0, MemFlags.ReadWrite, result))
        {
            if (OpenClInterfaceImplementation.CreateMemObjects<float>(cl, context, memObjects, false, 1, MemFlags.ReadOnly | MemFlags.CopyHostPtr, args.SelectMany(obj => obj.Flatten()).ToArray()))
            {
                if (OpenClInterfaceImplementation.SetKernelArgsMemory(cl, kernel, memObjects, new int[] { 0, 1 }) &&
                    OpenClInterfaceImplementation.SetKernelArgsVariables(cl, kernel, valueObjects, new int[] { 2, 3 }))
                {
                    if (Run(globalWorkSize, localWorkSize, result.Length, memObjects, out result))
                    {
                        return args.Select((obj, index) => {
                            obj.acceleration = result[index];
                            return obj;
                        }).ToList();

                    }
                }
            }
        }
        
        return args;
    }
}