using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


public class ObjectManager : MonoBehaviour
{
    [SerializeField]
    private List<PrefabEntry> prefabList = new();
    public Dictionary<string, GameObject> prefabs;

    public Slider timeDilationSlider;

    private float timeDilationValue;
    private readonly int nrSteps = 15;

    public CelestialBodyManager celestialBodyManager;

    private AccelerationRunner universalAttraction;
    private PathRunner movementPathRunner;

    public GameObject genericDataObject;

    private void Awake()
    {
        CreateConstantsClass();
        prefabs = new Dictionary<string, GameObject>();
        foreach (var entry in prefabList)
        {
            prefabs[entry.key] = entry.value;
        }
        celestialBodyManager = new CelestialBodyManager(genericDataObject, prefabs);

        universalAttraction = new AccelerationRunner("OpenCL_ComputeAcceleration", "universal_attraction_force");
        movementPathRunner = new PathRunner("OpenCL_ComputePath", "compute_movement_path");
    }

    void Start()
    {
        timeDilationSlider.onValueChanged.AddListener(delegate {
            timeDilationValue = timeDilationSlider.value;
        });
        celestialBodyManager.myObjectBodies = computePhysics(celestialBodyManager.myObjectBodies);
    }

    void Update()
    {
        if (Mathf.Abs(timeDilationValue) > Mathf.Epsilon)
        {
            celestialBodyManager.myObjectBodies = computePhysics(celestialBodyManager.myObjectBodies);
        }
        else
        {
            celestialBodyManager.myObjectBodies = computePath(celestialBodyManager.myObjectBodies);
        }

        celestialBodyManager.UpdateGraphics(Camera.main, timeDilationValue);
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
        returnValues = universalAttraction.Update(returnValues);
        return returnValues;
    }

    private List<OpenClBodyObject> computePath(List<OpenClBodyObject> inputBodies)
    {
        List<OpenClBodyObject> returnValues = inputBodies.Select(obj => obj.DeepClone()).ToList();
        returnValues = movementPathRunner.Update(returnValues, nrSteps, Camera.main);
        return returnValues;
    }

    private void CreateConstantsClass()
    {
#if UNITY_EDITOR
        TextAsset[] clFiles = Resources.LoadAll<TextAsset>("OpenCL_Scripts");
        TextAsset constantFile = Array.FindAll(clFiles, s => s.name.Contains("Constants"))[0];

        string outputPath = "./Assets/Scripts/DataTypes/Constants.cs"; // Output path for the generated C# class

        StringBuilder classBuilder = new StringBuilder();

        // Start of the class
        classBuilder.AppendLine("public static class Constants");
        classBuilder.AppendLine("{");

        Regex definePattern = new Regex(@"^\s*#define\s+(\w+)\s+([\d.e+-]+f?)\s*(?://\s*(.*))?$");
        Regex constPattern = new Regex(@"^\s*const\s+\w+\s+(\w+)\s*=\s*([\d.e+-]+f?);\s*(?://\s*(.*))?$");


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

                classBuilder.AppendLine("    /// <summary>");
                classBuilder.AppendLine($"    /// It is measured in: {comment}");
                classBuilder.AppendLine("    /// </summary>");

                if (value.ToLower().Contains("e"))
                {
                    classBuilder.AppendLine($"    public const {constType} {name} = {value};");
                }
                else if (value.EndsWith("f"))
                {
                    classBuilder.AppendLine($"    public const {constType} {name} = {value};");
                }
                else
                {
                    classBuilder.AppendLine($"    public const {constType} {name} = {value}f;");
                }
                classBuilder.AppendLine($"\n");
            }
        }

        // End of the class
        classBuilder.AppendLine("}");
        classBuilder.AppendLine($"\n");
        string oldFile = File.ReadAllText(outputPath, Encoding.UTF8);

        if (oldFile != classBuilder.ToString())
        {
            File.WriteAllText(outputPath, classBuilder.ToString());
            UnityEngine.Debug.Log($"Constants class has been updated to {outputPath}");
        }
#endif
    }
}