using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> executionQueue = new Queue<Action>();
    private static readonly object lockObject = new object();
    private static bool executing = false;

    // Singleton instance
    private static UnityMainThreadDispatcher instance;

    private void Awake()
    {
        // Ensure only one instance exists
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        ExecuteActions();
    }

    private void ExecuteActions()
    {
        lock (lockObject)
        {
            while (executionQueue.Count > 0)
            {
                Action action = executionQueue.Dequeue();
                action?.Invoke();
            }
            executing = false;
        }
    }

    public static void Enqueue(Action action)
    {
        lock (lockObject)
        {
            executionQueue.Enqueue(action);
            if (!executing)
            {
                executing = true;
                // Ensure the UnityMainThreadDispatcher instance exists
                if (instance == null)
                {
                    GameObject dispatcherObject = new GameObject("UnityMainThreadDispatcher");
                    instance = dispatcherObject.AddComponent<UnityMainThreadDispatcher>();
                }
            }
        }
    }

    public static void RunOnMainThread(Action action)
    {
        if (IsMainThread())
        {
            action();
        }
        else
        {
            Enqueue(action);
        }
    }

    private static bool IsMainThread()
    {
        return Thread.CurrentThread == System.Threading.Thread.CurrentThread;
    }

    public static void DestroyGameObject(GameObject gameObject)
    {
        RunOnMainThread(() => GameObject.Destroy(gameObject));
    }

    public static void DestroyGameObjectDelayed(GameObject gameObject, float delay)
    {
        RunOnMainThread(() => GameObject.Destroy(gameObject, delay));
    }

    public static void SetGameObjectActive(GameObject gameObject, bool value)
    {
        RunOnMainThread(() => gameObject.SetActive(value));
    }
}