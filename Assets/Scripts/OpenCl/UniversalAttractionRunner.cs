using Silk.NET.OpenCL;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UniversalAttractionRunner : OpenCLRunner<Vector4, OpenClBodies>
{
    public UniversalAttractionRunner(string filePath, string functionName) : base(filePath, functionName)
    {
    }

    public override void Update(OpenClBodies openClBodies, params object[] additionalParameters)
    {
        float deviationThreshold = 1f;
        List<OpenClBodyObject> pointsToUpdate = new List<OpenClBodyObject>();

        for (int index = 0; index < openClBodies.myObjectBodies.Count; index++)
        {
            if(openClBodies.myObjectBodies[index].pathPoints == null)
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
                Camera cam = (Camera)additionalParameters[0];
                if (dst > deviationThreshold || Vector3.Distance(cam.transform.position, objectToUpdate.position) < cam.farClipPlane)
                {
                    pointsToUpdate.Add(objectToUpdate);
                }
            }

        }

        int argsLength = pointsToUpdate.Count;

        if (argsLength == 0) return;

        nint[] memObjects = new nint[2];
        int[] valueObjects = new int[2] { argsLength, pointsToUpdate[0].Flatten().Count()};
        Vector4[] result = new Vector4[(nuint)argsLength];

        nuint[] globalWorkSize = new nuint[1] { (nuint)argsLength };
        nuint[] localWorkSize = new nuint[1] { 1 };

        if (OpenCLInterfaceImplementation.CreateMemObjects(cl, context, memObjects, true, 0, MemFlags.ReadWrite, result)
            && OpenCLInterfaceImplementation.CreateMemObjects(cl, context, memObjects, false, 1, MemFlags.ReadOnly | MemFlags.CopyHostPtr, openClBodies.Flatten().ToArray())
            && OpenCLInterfaceImplementation.SetKernelArgsMemory(cl, kernel, memObjects, new int[] { 0, 1 })
            && OpenCLInterfaceImplementation.SetKernelArgsVariables(cl, kernel, valueObjects, new int[] { 2, 3 })
            && Run(globalWorkSize, localWorkSize, result.Length, memObjects, 0, out result))
        {
            for (int index = 0; index < argsLength; index++)
            {
                int index_openCL = openClBodies.myObjectBodies.IndexOf(pointsToUpdate[index]);
                OpenClBodyObject obj = openClBodies.myObjectBodies[index_openCL];
                obj.acceleration = result[index];
                openClBodies.myObjectBodies[index_openCL] = obj;
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