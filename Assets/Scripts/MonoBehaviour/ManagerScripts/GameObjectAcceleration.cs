using System.Collections.Generic;
using UnityEngine;

public class GameObjectAcceleration : ObjectRunnerBase
{
    private UniversalAttractionRunner universalAttraction;
    Dictionary<string, Vector3> taskAcceleration;

    protected override void SpecificStart()
    {
        universalAttraction = new UniversalAttractionRunner("OpenCL_ComputeAcceleration", "universal_attraction_force");
    }

    protected override void RunUpdate()
    {
        universalAttraction.Update(bodies, gameObjectOffset);
    }

    protected override void RunGameObjectUpdate(){}
}
