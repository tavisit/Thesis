using Silk.NET.OpenCL;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public static class OpenCLInterfaceImplementation
{
    public static unsafe bool CreateMemObjects<T>(CL cl, nint context, nint[] memObjects,
                                                   bool nullPtr, int position, MemFlags memFlags, T[] array)
    {
        if (!typeof(T).IsValueType)
        {
            Debug.LogError("T " + typeof(T) + " must be a value type.");
            return false;
        }

        try
        {
            void* ptr = null;

            // Check if nullPtr is false before attempting to fix the memory
            if (!nullPtr)
            {
                ptr = Marshal.UnsafeAddrOfPinnedArrayElement(array, 0).ToPointer();
            }

            memObjects[position] = cl.CreateBuffer(context, memFlags, (nuint)(Marshal.SizeOf<T>() * array.Length), nullPtr? null:ptr, out var errorCode);
            if (memObjects[position] == IntPtr.Zero)
            {
                Debug.LogError("Error creating memory objects. Error code: " + errorCode);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError("Exception occurred: " + ex.Message);
            return false;
        }
    }

    public static unsafe bool SetKernelArgsMemory(CL cl, nint kernel, nint[] memObjects, int[] memObjectsPositions)
    {
        int errNum = 0;
        for (int i = 0; i < memObjects.Length; i++)
        {
            int position = memObjectsPositions[i];
            errNum |= cl.SetKernelArg(kernel, (uint)position, (nuint)sizeof(nint), memObjects[i]);
        }
        if (errNum != (int)ErrorCodes.Success)
        {
            Debug.Log("Error setting kernel arguments memory.");
            return false;
        }
        return true;
    }

    public static unsafe bool SetKernelArgsVariables<T>(CL cl, nint kernel, T[] valueObjects, int[] valueObjectsPositions) where T : unmanaged
    {
        int errNum = 0;

        for (uint i = 0; i < valueObjects.Length; i++)
        {
            int position = valueObjectsPositions[i];
            T currentValue = valueObjects[i];
            errNum |= cl.SetKernelArg(kernel, (uint)position, (nuint)Marshal.SizeOf(currentValue), &currentValue);
        }

        if (errNum != (int)ErrorCodes.Success)
        {
            Debug.Log("Error setting kernel arguments.");
            return false;
        }
        return true;
    }

    public static unsafe CL Enqueue(CL cl, nint commandQueue, nint kernel, nuint[] globalWorkSize, nuint[] localWorkSize, nint[] memObjects)
    {
        int errNum = (int)ErrorCodes.InvalidQueueProperties;
        try
        {
            fixed (nuint* ptr = globalWorkSize)
            {
                fixed (nuint* ptr2 = localWorkSize)
                {
                    errNum = cl.EnqueueNdrangeKernel(commandQueue, kernel, 1, null, ptr, ptr2, 0, null, null);

                }
            }
        }
        catch (Exception e) { Debug.Log("Error queuing kernel for execution." + e); };

        if (errNum != (int)ErrorCodes.Success)
        {
            Debug.Log("Error queuing kernel for execution.");
        }
        return cl;
    }
    public static unsafe CL ReadBuffer<T>(CL cl, nint commandQueue, nint[] memObjects, int position, out T[] result, int resultSize)
    {
        result = new T[resultSize]; 
        if (!typeof(T).IsValueType)
        {
            Debug.LogError("T " + typeof(T) + " must be a value type.");
            return cl;
        }
        try
        {
            void* ptr = null;
            ptr = Marshal.UnsafeAddrOfPinnedArrayElement(result, 0).ToPointer();
            int errNum = cl.EnqueueReadBuffer(commandQueue, memObjects[position], true, 0, (nuint)(Marshal.SizeOf(typeof(T)) * resultSize), ptr, 0, null, null);
            if (errNum != (int)ErrorCodes.Success)
            {
                Debug.Log("Error reading result buffer.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Exception occurred: " + ex.Message);
        }
        return cl;
    }
}