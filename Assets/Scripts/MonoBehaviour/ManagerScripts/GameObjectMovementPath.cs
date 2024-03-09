using System.Collections.Generic;
using UnityEngine;

public class GameObjectMovementPath : ObjectRunnerBase
{
    private MovementPathRunner movementPathRunner;
    Dictionary<string, List<Vector3>> taskMovementPath;

    readonly int nr_steps = 10;

    protected override void SpecificStart()
    {
        movementPathRunner = new MovementPathRunner("OpenCL_ComputePath", "compute_movement_path");
    }

    protected override void RunUpdate()
    {
        if(index_update % 100 == 0 || System.Math.Abs(index_update) < 2)
        {
            movementPathRunner.Update(bodies, gameObjectOffset, nr_steps);
        }
    }

    protected override void RunGameObjectUpdate(){}
}
