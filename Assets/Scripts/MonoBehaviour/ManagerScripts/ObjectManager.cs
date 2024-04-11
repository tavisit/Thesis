using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


public class ObjectManager : MonoBehaviour
{
    [SerializeField]
    private List<PrefabEntry> prefabList = new();
    public Dictionary<string, GameObject> prefabs;

    public Slider timeDilationSlider;

    private float timeDilationValue;
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
    }

    void Start()
    {
        timeDilationSlider.onValueChanged.AddListener(delegate {
            timeDilationValue = timeDilationSlider.value;
        });
        openClBodies.myObjectBodies = computePhysics(openClBodies.myObjectBodies);
    }

    void Update()
    {
        if (Mathf.Abs(timeDilationValue) > Mathf.Epsilon)
        {
            openClBodies.myObjectBodies = computePhysics(openClBodies.myObjectBodies);
        }
        else
        {
            openClBodies.myObjectBodies = computePath(openClBodies.myObjectBodies);
        }

        openClBodies.UpdateGraphics(Camera.main, prefabs, timeDilationValue);
    }
     
    private List<OpenClBodyObject> computePhysics(List<OpenClBodyObject> inputBodies)
    {
        List<OpenClBodyObject> returnValues = inputBodies.Select(obj => obj.DeepClone()).ToList();
        returnValues = computeAttraction(returnValues);
        returnValues = computePath(returnValues);
        return returnValues;
    }

    private List<OpenClBodyObject> computeAttraction(List<OpenClBodyObject> inputBodies)
    {
        List<OpenClBodyObject> returnValues = inputBodies.Select(obj => obj.DeepClone()).ToList();
        watch.Restart();

        returnValues = universalAttraction.Update(returnValues);

        watch.Stop();
        UnityEngine.Debug.Log($"universalAttraction : {watch.ElapsedMilliseconds}");
        return returnValues;
    }

    private List<OpenClBodyObject> computePath(List<OpenClBodyObject> inputBodies)
    {
        List<OpenClBodyObject> returnValues = inputBodies.Select(obj => obj.DeepClone()).ToList();
        watch.Restart();

        returnValues = movementPathRunner.Update(returnValues, nrSteps, Camera.main);

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