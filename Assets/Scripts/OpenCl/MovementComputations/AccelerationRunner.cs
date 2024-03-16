using Silk.NET.OpenCL;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AccelerationRunner : MovementComputationsBaseRunner
{
    public AccelerationRunner(string filePath, string functionName) : base(filePath, functionName)
    {
    }

    public override void Update(OpenClBodies args, params object[] additionalParameters)
    {
        List<OpenClBodyObject> pointsToUpdate = SimplifyUpdateObjects(args);

        int argsLength = pointsToUpdate.Count;

        if (argsLength == 0) return;

        nint[] memObjects = new nint[2];
        int[] valueObjects = new int[2] { argsLength, pointsToUpdate[0].Flatten().Count() };
        Vector4[] result = new Vector4[(nuint)argsLength];

        nuint[] globalWorkSize = new nuint[1] { (nuint)argsLength };
        nuint[] localWorkSize = new nuint[1] { 1 };

        if (OpenCLInterfaceImplementation.CreateMemObjects(cl, context, memObjects, true, 0, MemFlags.ReadWrite, result)
            && OpenCLInterfaceImplementation.CreateMemObjects(cl, context, memObjects, false, 1, MemFlags.ReadOnly | MemFlags.CopyHostPtr, args.Flatten().ToArray())
            && OpenCLInterfaceImplementation.SetKernelArgsMemory(cl, kernel, memObjects, new int[] { 0, 1 })
            && OpenCLInterfaceImplementation.SetKernelArgsVariables(cl, kernel, valueObjects, new int[] { 2, 3 })
            && Run(globalWorkSize, localWorkSize, result.Length, memObjects, 0, out result))
        {
            for (int index = 0; index < argsLength; index++)
            {
                int index_openCL = args.myObjectBodies.IndexOf(pointsToUpdate[index]);
                OpenClBodyObject obj = args.myObjectBodies[index_openCL];
                obj.acceleration = result[index];
                args.myObjectBodies[index_openCL] = obj;
            }
        }
    }
}