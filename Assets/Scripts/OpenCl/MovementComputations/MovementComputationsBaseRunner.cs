using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public abstract class MovementComputationsBaseRunner : OpenCLRunner<Vector4, OpenClBodies, OpenClBodyObject>
{
    public MovementComputationsBaseRunner(string filePath, string functionName) : base(filePath, functionName)
    {
    }

    protected override ConcurrentBag<OpenClBodyObject> SimplifyUpdateObjects(OpenClBodies args)
    {
        float deviationThreshold = 1f;

        ConcurrentBag<OpenClBodyObject> pointsToUpdate = new ConcurrentBag<OpenClBodyObject>();

        // Parallelize the loop
        Parallel.For(0, args.myObjectBodies.Count, index =>
        {
            OpenClBodyObject objectToUpdate = args.myObjectBodies[index];

            if (objectToUpdate.pathPoints == null || objectToUpdate.pathPoints.Count < 2)
            {
                if (objectToUpdate.pathPoints == null)
                {
                    objectToUpdate.pathPoints = new List<Vector3>();
                }
                pointsToUpdate.Add(objectToUpdate);
                return;
            }

            float distanceToPath = PointToRayDistance(objectToUpdate.position, objectToUpdate.pathPoints[0], objectToUpdate.pathPoints[1]);
            float distanceToObjectFromStart = Vector3.Distance(objectToUpdate.position, objectToUpdate.pathPoints[0]);
            float pathLength = Vector3.Distance(objectToUpdate.pathPoints[1], objectToUpdate.pathPoints[0]);

            if (distanceToPath > deviationThreshold || distanceToObjectFromStart > pathLength)
            {
                pointsToUpdate.Add(objectToUpdate);
            }
        });

        return pointsToUpdate;
    }

    protected static float PointToRayDistance(Vector3 point, Vector3 origin, Vector3 target)
    {
        var ray = new Ray(origin, target - origin);
        var cross = Vector3.Cross(ray.direction, point - ray.origin);

        return cross.magnitude;
    }
}
