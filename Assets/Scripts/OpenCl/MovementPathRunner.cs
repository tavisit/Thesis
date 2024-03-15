using Silk.NET.OpenCL;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovementPathRunner : OpenCLRunner<Vector4, OpenClBodies>
{
    public MovementPathRunner(string filePath, string functionName) : base(filePath, functionName)
    {
    }

    public override void Update(OpenClBodies openClBodies, params object[] additionalParameters)
    {
        float deviationThreshold = 1f;

        List<OpenClBodyObject> pointsToUpdate = new List<OpenClBodyObject>();

        for (int index = 0; index < openClBodies.myObjectBodies.Count; index++)
        {
            if (openClBodies.myObjectBodies[index].pathPoints == null)
            {
                openClBodies.myObjectBodies[index].pathPoints = new List<Vector3>();
                pointsToUpdate.Add(openClBodies.myObjectBodies[index]);
            }
            else if (openClBodies.myObjectBodies[index].pathPoints.Count == 0)
            {
                pointsToUpdate.Add(openClBodies.myObjectBodies[index]);
            }
            else
            {
                OpenClBodyObject objectToUpdate = openClBodies.myObjectBodies[index];
                float dst = PointToRayDistance(objectToUpdate.pathPoints[0], objectToUpdate.pathPoints[1], objectToUpdate.position);
                if (dst > deviationThreshold)
                {
                    pointsToUpdate.Add(objectToUpdate);
                }
            }
        }

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
                int index_openCL = openClBodies.myObjectBodies.IndexOf(pointsToUpdate[index]);
                openClBodies.myObjectBodies[index_openCL].pathPoints.Clear();
                for (int step = 0; step < steps; step++)
                {
                    openClBodies.myObjectBodies[index_openCL].pathPoints.Add(result[index * steps + step]);
                }
            }
        }
    }
    public static float PointToRayDistance(Vector3 point, Vector3 origin, Vector3 target)
    {
        var ray = new Ray(origin, target - origin);
        var cross = Vector3.Cross(ray.direction, point - ray.origin);

        return cross.magnitude;
    }
}
