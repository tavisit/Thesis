using System;
using UnityEngine;
using System.Threading;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static Action[] executionArray = new Action[64]; // Adjust the initial capacity as needed
    private static int executionCount = 0;
    private static readonly object lockObject = new();
    private static bool executing = false;

    private static int mainThreadId;
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
        mainThreadId = Thread.CurrentThread.ManagedThreadId;
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
            if (executionCount == 0)
            {
                executing = false;
                return;
            }

            // Execute actions
            for (int i = 0; i < executionCount; i++)
            {
                executionArray[i]?.Invoke();
                executionArray[i] = null; // Clear the action after execution
            }
            executionCount = 0;
            executing = false;
        }
    }

    public static void Enqueue(Action action)
    {
        lock (lockObject)
        {
            // Expand the array if needed
            if (executionCount >= executionArray.Length)
            {
                Array.Resize(ref executionArray, executionArray.Length * 2);
            }
            executionArray[executionCount++] = action;

            if (!executing)
            {
                executing = true;
                // Ensure the UnityMainThreadDispatcher instance exists
                if (instance == null)
                {
                    GameObject dispatcherObject = new("UnityMainThreadDispatcher");
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

    public static bool IsMainThread()
    {
        return Thread.CurrentThread.ManagedThreadId == mainThreadId;
    }
}