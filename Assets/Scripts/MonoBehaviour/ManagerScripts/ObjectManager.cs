using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class ObjectManager : MonoBehaviour
{
    public GameObject prefab;
    protected Slider timeDilationSlider;
    private readonly int nr_steps = 10;

    private OpenClBodies openClBodies;

    private UniversalAttractionRunner universalAttraction;
    private MovementPathRunner movementPathRunner;

    private Stopwatch watch;


    void Start()
    {
        openClBodies = new OpenClBodies();


        universalAttraction = new UniversalAttractionRunner("OpenCL_ComputeAcceleration", "universal_attraction_force");
        movementPathRunner = new MovementPathRunner("OpenCL_ComputePath", "compute_movement_path");


        watch = Stopwatch.StartNew();

        timeDilationSlider = GameObject.Find("TimeDillationSlider")?.GetComponent<Slider>();

        universalAttraction.Update(openClBodies);
        movementPathRunner.Update(openClBodies, nr_steps);
    }

    void Update()
    {
        if (timeDilationSlider == null)
        {
            timeDilationSlider = GameObject.Find("TimeDilationSlider")?.GetComponent<Slider>();
        }
        else
        {
            if (Mathf.Abs(timeDilationSlider.value) > Mathf.Epsilon)
            {
                watch.Restart();

                universalAttraction.Update(openClBodies, Camera.main);

                watch.Stop();
                UnityEngine.Debug.Log($"universalAttraction : {watch.ElapsedMilliseconds}");
                watch.Restart();


                watch.Restart();

                movementPathRunner.Update(openClBodies, nr_steps);

                watch.Stop();
                UnityEngine.Debug.Log($"movementPathRunner : {watch.ElapsedMilliseconds}");
            }

            openClBodies.UpdateGraphics(Camera.main, prefab, timeDilationSlider.value);
        }
    }
}