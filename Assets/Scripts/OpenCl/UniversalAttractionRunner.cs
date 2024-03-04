using Silk.NET.OpenCL;
using System.Collections.Generic;
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
            if (OpenClInterfaceImplementation.CreateMemObjects(cl, context, memObjects, false, 1, MemFlags.ReadOnly | MemFlags.CopyHostPtr, FlattenList(args).ToArray()))
            {
                if (OpenClInterfaceImplementation.SetKernelArgs(cl, kernel, memObjects, new int[] { 0, 1 }, valueObjects, new int[] { 2, 3 }))
                {
                    if (Run(globalWorkSize, localWorkSize, result.Length, memObjects, out result))
                    {
                        List<OpenClBodyObject> returnList = new List<OpenClBodyObject>();
                        for (int i = 0; i < result.Length; i++)
                        {
                            OpenClBodyObject myObject = args[i];
                            myObject.acceleration = result[i];
                            returnList.Add(myObject);
                        }
                        return returnList;
                    }
                }
            }
        }
        
        return args;
    }

    public List<float> FlattenList(List<OpenClBodyObject> args)
    {
        List<float> flattenedList = new List<float>();

        foreach (var obj in args)
        {
            float[] flattenedObject = obj.Flatten().ToArray();
            flattenedList.AddRange(flattenedObject);
        }

        return flattenedList;
    }
}