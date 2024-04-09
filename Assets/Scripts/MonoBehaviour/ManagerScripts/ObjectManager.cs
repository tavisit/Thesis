using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ObjectManager : MonoBehaviour
{
    [SerializeField]
    private List<PrefabEntry> prefabList = new();
    public Dictionary<string, GameObject> prefabs;


    protected Slider timeDilationSlider;
    private readonly int nrSteps = 15;

    public OpenClBodies openClBodies;

    private AccelerationRunner universalAttraction;
    private PathRunner movementPathRunner;

    private Stopwatch watch;
    private void Awake()
    {
        CreateConstantsClass();
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
        openClBodies = computePhysics(openClBodies);
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
                openClBodies = computePhysics(openClBodies);
            }

            openClBodies.UpdateGraphics(Camera.main, prefabs, timeDilationSlider.value);

        }
    }

    private OpenClBodies computePhysics(OpenClBodies openClBodies)
    {
        OpenClBodies returnValues = openClBodies.DeepClone();
        watch.Restart();

        universalAttraction.Update(returnValues);

        watch.Stop();
        UnityEngine.Debug.Log($"universalAttraction : {watch.ElapsedMilliseconds}");
        watch.Restart();


        watch.Restart();

        movementPathRunner.Update(returnValues, nrSteps, Camera.main);

        watch.Stop();
        UnityEngine.Debug.Log($"movementPathRunner : {watch.ElapsedMilliseconds}");

        return returnValues;
    }

    private void CreateConstantsClass()
    {
        TextAsset[] clFiles = Resources.LoadAll<TextAsset>("OpenCL_Scripts");
        TextAsset constantFile = Array.FindAll(clFiles, s => s.name.Contains("Constants"))[0];

        string outputPath = "./Assets/Scripts/DataTypes/Constants.cs"; // Output path for the generated C# class

        StringBuilder classBuilder = new StringBuilder();

        // Start of the class
        classBuilder.AppendLine("public static class Constants");
        classBuilder.AppendLine("{");

        Regex definePattern = new Regex(@"^\s*#define\s+(\w+)\s+([\d.e+-]+f?)\s*(//\s*(.*))?$");
        Regex constPattern = new Regex(@"^\s*const\s+\w+\s+(\w+)\s*=\s*([\d.e+-]+f?);\s*(//\s*(.*))?$");


        foreach (var line in constantFile.text.Split("\r\n"))
        {
            var defineMatch = definePattern.Match(line);
            var constMatch = constPattern.Match(line);

            if (defineMatch.Success || constMatch.Success)
            {
                var name = defineMatch.Success ? defineMatch.Groups[1].Value : constMatch.Groups[1].Value;
                var value = defineMatch.Success ? defineMatch.Groups[2].Value : constMatch.Groups[2].Value;
                var comment = defineMatch.Success ? defineMatch.Groups[3].Value : constMatch.Groups[3].Value;

                string constType = "float";
                if (value.ToLower().Contains("e"))
                {
                    classBuilder.AppendLine($"    public const {constType} {name} = {value}; {comment}");
                }
                else if (value.EndsWith("f"))
                {
                    classBuilder.AppendLine($"    public const {constType} {name} = {value}; {comment}");
                }
                else
                {
                    classBuilder.AppendLine($"    public const {constType} {name} = {value}f; {comment}");
                }
            }
        }

        // End of the class
        classBuilder.AppendLine("}");
        string oldFile = File.ReadAllText(outputPath, Encoding.UTF8);

        if (oldFile != classBuilder.ToString())
        {
            File.WriteAllText(outputPath, classBuilder.ToString());
            UnityEngine.Debug.Log($"Constants class has been updated to {outputPath}");
        }
    }
}