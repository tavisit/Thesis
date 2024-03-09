using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;

public abstract class ObjectRunnerBase : MonoBehaviour
{
    protected List<GameObject> bodies;

    public int gameObjectOffset = 10;

    protected float timeElapsed = 0.0f;
    protected float index_update = 0;

    Stopwatch watch;

    protected abstract void RunUpdate();
    protected abstract void RunGameObjectUpdate();
    protected abstract void SpecificStart();


    void Start()
    {
        bodies = new List<GameObject>();
        SpecificStart();
        watch = Stopwatch.StartNew();
    }


    void Update()
    {
        index_update++;
        watch.Restart();

        bodies = GameObject.FindGameObjectsWithTag("Body").ToList();
        RunUpdate();
        RunGameObjectUpdate();

        watch.Stop();
        timeElapsed += watch.ElapsedMilliseconds;
        UnityEngine.Debug.Log($"{GetType().Name} : {timeElapsed / index_update}");
    }

    public void ClearLog()
    {
        var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }
}