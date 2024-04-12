﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Silk.NET.OpenCL;
using UnityEngine;

public class PathRunner : MovementComputationsBaseRunner
{
    private static Plane[] frustumPlanes;
    public PathRunner(string filePath, string functionName) : base(filePath, functionName)
    {
    }

    public override List<OpenClBodyObject> Update(List<OpenClBodyObject> args, params object[] additionalParameters)
    {
        List<OpenClBodyObject> pointsToUpdate = SimplifyByView(SimplifyUpdateObjects(args), (Camera)additionalParameters[1]);

        if (pointsToUpdate == null) return args;

        int argsLength = pointsToUpdate.Count;

        if (argsLength == 0) return args;

        int steps = (int)additionalParameters[0];
        nint[] memObjects = new nint[3];
        int[] intObjects = new int[3] { argsLength, pointsToUpdate.FirstOrDefault().Flatten().Count(), steps };
        Vector4[] result = new Vector4[(nuint)(argsLength * steps)];

        nuint[] globalWorkSize = new nuint[1] { (nuint)argsLength };
        nuint[] localWorkSize = new nuint[1] { 1 };

        if (OpenCLInterfaceImplementation.CreateMemObjects(cl, context, memObjects, true, 0, MemFlags.ReadWrite, result)
            && OpenCLInterfaceImplementation.CreateMemObjects(cl, context, memObjects, false, 1, MemFlags.ReadOnly | MemFlags.CopyHostPtr, pointsToUpdate.SelectMany(obj => obj.Flatten()).ToArray())
            && OpenCLInterfaceImplementation.CreateMemObjects(cl, context, memObjects, false, 2, MemFlags.ReadOnly | MemFlags.CopyHostPtr, args.SelectMany(obj => obj.Flatten()).ToArray())
            && OpenCLInterfaceImplementation.SetKernelArgsMemory(cl, kernel, memObjects, new int[] { 0, 1, 2 })
            && OpenCLInterfaceImplementation.SetKernelArgsVariables(cl, kernel, intObjects, new int[] { 3, 4, 5 })
            && Run(globalWorkSize, localWorkSize, result.Length, memObjects, 0, out result))
        {
            Parallel.For(0, pointsToUpdate.Count(), index_openCL =>
            {
                int index_myObjectBodies = args.IndexOf(pointsToUpdate[index_openCL]);
                if (index_myObjectBodies >= 0)
                {
                    List<Vector3> updatedPathPoints = new();
                    for (int step = 0; step < steps; step++)
                    {
                        updatedPathPoints.Add(result[index_openCL * steps + step]);
                    }
                    args[index_myObjectBodies].pathPoints.Clear();
                    args[index_myObjectBodies].pathPoints.AddRange(updatedPathPoints);
                }
            });
        }

        return args;
    }
    private List<OpenClBodyObject> SimplifyByView(List<OpenClBodyObject> args, Camera cam)
    {
        List<OpenClBodyObject> pointsToUpdate = new();

        foreach (var obj in args)
        {
            if (IsInFrustumAndInView(cam, obj.position))
            {
                pointsToUpdate.Add(obj);
            }
        }

        return pointsToUpdate;
    }

    private static bool IsInFrustumAndInView(Camera camera, Vector3 position)
    {
        frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
        Bounds bounds = new(position, Vector3.zero);
        return GeometryUtility.TestPlanesAABB(frustumPlanes, bounds) && ViewHelper.IsInView(camera, position);
    }
}
