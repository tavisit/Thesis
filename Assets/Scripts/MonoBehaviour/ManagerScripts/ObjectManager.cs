using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class ObjectManager : MonoBehaviour
{
    public GameObject prefab;
    protected Slider timeDilationSlider;
    private readonly int nr_steps = 5;

    public OpenClBodies openClBodies;

    private AccelerationRunner universalAttraction;
    private PathRunner movementPathRunner;

    private Stopwatch watch;


    void Start()
    {
        openClBodies = new OpenClBodies();

        universalAttraction = new AccelerationRunner("OpenCL_ComputeAcceleration", "universal_attraction_force");
        movementPathRunner = new PathRunner("OpenCL_ComputePath", "compute_movement_path");

        watch = Stopwatch.StartNew();

        timeDilationSlider = GameObject.Find("TimeDillationSlider")?.GetComponent<Slider>();

        universalAttraction.Update(openClBodies);
        movementPathRunner.Update(openClBodies, nr_steps, Camera.main);
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

                movementPathRunner.Update(openClBodies, nr_steps, Camera.main);

                watch.Stop();
                UnityEngine.Debug.Log($"movementPathRunner : {watch.ElapsedMilliseconds}");

            }

            openClBodies.UpdateGraphics(Camera.main, prefab, timeDilationSlider.value);

        }
    }
}