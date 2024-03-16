using Silk.NET.OpenCL;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathRunner : MovementComputationsBaseRunner
{
    public PathRunner(string filePath, string functionName) : base(filePath, functionName)
    {
    }

    public override void Update(OpenClBodies args, params object[] additionalParameters)
    {
        List<OpenClBodyObject> pointsToUpdate = SimplifyUpdateObjects(args);
        pointsToUpdate = SimplifyByView(pointsToUpdate, (Camera)additionalParameters[1]);

        int argsLength = pointsToUpdate.Count;

        if (argsLength == 0) return;

        int steps = (int)additionalParameters[0];
        nint[] memObjects = new nint[2];
        int[] intObjects = new int[3] { argsLength, pointsToUpdate[0].Flatten().Count(), steps };
        Vector4[] result = new Vector4[(nuint)(argsLength * steps)];

        nuint[] globalWorkSize = new nuint[1] { (nuint)argsLength };
        nuint[] localWorkSize = new nuint[1] { 1 };

        if (OpenCLInterfaceImplementation.CreateMemObjects(cl, context, memObjects, true, 0, MemFlags.ReadWrite, result)
            && OpenCLInterfaceImplementation.CreateMemObjects(cl, context, memObjects, false, 1, MemFlags.ReadOnly | MemFlags.CopyHostPtr, pointsToUpdate.SelectMany(obj => obj.Flatten()).ToArray())
            && OpenCLInterfaceImplementation.SetKernelArgsMemory(cl, kernel, memObjects, new int[] { 0, 1 })
            && OpenCLInterfaceImplementation.SetKernelArgsVariables(cl, kernel, intObjects, new int[] { 2, 3, 4 })
            && Run(globalWorkSize, localWorkSize, result.Length, memObjects, 0, out result))
        {
            for (int index = 0; index < argsLength; index++)
            {
                int index_openCL = args.myObjectBodies.IndexOf(pointsToUpdate[index]);
                args.myObjectBodies[index_openCL].pathPoints.Clear();
                for (int step = 0; step < steps; step++)
                {
                    args.myObjectBodies[index_openCL].pathPoints.Add(result[index * steps + step]);
                }
            }
        }
    }
    private List<OpenClBodyObject> SimplifyByView(List<OpenClBodyObject> args, Camera cam)
    {
        List<OpenClBodyObject> pointsToUpdate = new();
        for (int i = 0; i < args.Count; i++)
        {
            if (ViewHelper.IsInView(cam, args[i].position))
            {
                pointsToUpdate.Add(args[i]);
            }
        }
        return pointsToUpdate;
    }
}
