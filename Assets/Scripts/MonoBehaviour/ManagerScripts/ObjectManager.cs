using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class ObjectManager : MonoBehaviour
{
    [SerializeField]
    private List<PrefabEntry> prefabList = new();
    public Dictionary<string, GameObject> prefabs;


    protected Slider timeDilationSlider;
    private readonly int nrSteps = 5;

    public OpenClBodies openClBodies;

    private AccelerationRunner universalAttraction;
    private PathRunner movementPathRunner;

    private Stopwatch watch;
    private void Awake()
    {
        prefabs = new Dictionary<string, GameObject>();
        foreach (var entry in prefabList)
        {
            prefabs[entry.key] = entry.value;
        }
        openClBodies = new OpenClBodies();

        universalAttraction = new AccelerationRunner("OpenCL_ComputeAcceleration", "universal_attraction_force");
        movementPathRunner = new PathRunner("OpenCL_ComputePath", "compute_movement_path");

        watch = Stopwatch.StartNew();

        timeDilationSlider = GameObject.Find("TimeDillationSlider")?.GetComponent<Slider>();
    }

    void Start()
    {
        universalAttraction.Update(openClBodies);
        movementPathRunner.Update(openClBodies, nrSteps, Camera.main);
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

                movementPathRunner.Update(openClBodies, nrSteps, Camera.main);

                watch.Stop();
                UnityEngine.Debug.Log($"movementPathRunner : {watch.ElapsedMilliseconds}");

            }

            openClBodies.UpdateGraphics(Camera.main, prefabs, timeDilationSlider.value);

        }
    }
}