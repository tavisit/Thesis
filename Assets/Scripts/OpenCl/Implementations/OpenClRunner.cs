using System;
using System.Collections.Generic;
using System.Linq;
using Silk.NET.OpenCL;
using UnityEngine;

public abstract class OpenCLRunner<T, P, Q>
{
    protected nint context;
    protected nint commandQueue;
    protected nint program;
    protected nint kernel;
    protected CL cl;

    protected OpenCLInterfaceImplementation openCLInterfaceImplementation;

    public OpenCLRunner(string filePath, string functionName)
    {
        openCLInterfaceImplementation = OpenCLInterfaceImplementation.Instance;

        cl = CL.GetApi();
        nint device = 0;

        context = CreateContext(cl);
        commandQueue = CreateCommandQueue(cl, context, ref device);
        program = CreateProgram(cl, context, device, filePath);
        kernel = CreateKernel(cl, program, functionName);
    }

    protected bool Run(nuint[] globalWorkSize, nuint[] localWorkSize, int resultSize, nint[] memObjects, int position, out T[] result)
    {
        cl = openCLInterfaceImplementation.Enqueue(cl, commandQueue, kernel, globalWorkSize, localWorkSize, memObjects);
        cl = openCLInterfaceImplementation.ReadBuffer(cl, commandQueue, memObjects, position, out result, resultSize);
        return true;
    }

    private unsafe nint CreateKernel(CL cl, nint program, string functionName)
    {
        nint kernel = cl.CreateKernel(program, functionName, null);
        if (kernel == IntPtr.Zero)
        {
            Debug.Log("Failed to create kernel");
        }
        return kernel;
    }

    private unsafe nint CreateCommandQueue(CL cl, nint context, ref nint device)
    {
        int errNum = cl.GetContextInfo(context, ContextInfo.Devices, 0, null, out nuint deviceBufferSize);
        if (errNum != (int)ErrorCodes.Success)
        {
            Debug.Log("Failed call to clGetContextInfo(...,GL_CONTEXT_DEVICES,...)");
            return IntPtr.Zero;
        }

        if (deviceBufferSize <= 0)
        {
            Debug.Log("No devices available.");
            return IntPtr.Zero;
        }

        nint[] devices = new nint[deviceBufferSize / (nuint)sizeof(nuint)];
        fixed (void* pValue = devices)
        {
            int er = cl.GetContextInfo(context, ContextInfo.Devices, deviceBufferSize, pValue, null);

        }
        if (errNum != (int)ErrorCodes.Success)
        {
            devices = null;
            Debug.Log("Failed to get device IDs");
            return IntPtr.Zero;
        }
        var commandQueue = cl.CreateCommandQueue(context, devices[0], CommandQueueProperties.None, null);
        if (commandQueue == IntPtr.Zero)
        {
            Debug.Log("Failed to create commandQueue for device 0");
            return IntPtr.Zero;
        }

        device = devices[0];
        return commandQueue;
    }

    private unsafe nint CreateContext(CL cl)
    {
        var errNum = cl.GetPlatformIDs(1, out nint firstPlatformId, out uint numPlatforms);
        if (errNum != (int)ErrorCodes.Success || numPlatforms <= 0)
        {
            Debug.Log("Failed to find any OpenCL platforms.");
            return IntPtr.Zero;
        }

        nint[] contextProperties = new nint[]
        {
                (nint)ContextProperties.Platform,
                firstPlatformId,
                0
        };

        fixed (nint* p = contextProperties)
        {
            var context = cl.CreateContextFromType(p, Silk.NET.OpenCL.DeviceType.Gpu, null, null, out errNum);
            if (errNum != (int)ErrorCodes.Success)
            {
                Debug.Log("Could not create GPU context, trying CPU...");

                context = cl.CreateContextFromType(p, Silk.NET.OpenCL.DeviceType.Cpu, null, null, out errNum);

                if (errNum != (int)ErrorCodes.Success)
                {
                    Debug.Log("Failed to create an OpenCL GPU or CPU context.");
                    return IntPtr.Zero;
                }
            }

            if (context == IntPtr.Zero)
            {
                Debug.Log("Failed to create OpenCL context.");
                return 0;
            }
            return context;
        }
    }
    private unsafe nint CreateProgram(CL cl, nint context, nint device, string fileName)
    {
        string clSource = LoadAndCombineClSources(fileName);

        var program = cl.CreateProgramWithSource(context, 1, new string[] { clSource }, null, null);
        if (program == IntPtr.Zero)
        {
            Debug.Log("Failed to create CL program from source.");
            return IntPtr.Zero;
        }

        var errNum = cl.BuildProgram(program, 0, null, (byte*)null, null, null);

        if (errNum != (int)ErrorCodes.Success)
        {
            _ = cl.GetProgramBuildInfo(program, device, ProgramBuildInfo.BuildLog, 0, null, out nuint buildLogSize);
            byte[] log = new byte[buildLogSize / (nuint)sizeof(byte)];
            fixed (void* pValue = log)
            {
                cl.GetProgramBuildInfo(program, device, ProgramBuildInfo.BuildLog, buildLogSize, pValue, null);
            }
            var build_log = System.Text.Encoding.UTF8.GetString(log);

            Debug.Log("=============== OpenCL Program Build Info ================");
            Debug.Log(build_log);

            cl.ReleaseProgram(program);
            return IntPtr.Zero;
        }

        return program;
    }

    private string LoadAndCombineClSources(string fileName)
    {
        TextAsset[] clFiles = Resources.LoadAll<TextAsset>("OpenCL_Scripts");

        TextAsset mainFile = clFiles.FirstOrDefault(s => s.name.Equals(fileName));
        if (mainFile == null)
        {
            Debug.LogError($"Cannot find {fileName} in Resources/OpenCL_Scripts/. " +
                $"All the files I have are: {string.Join(", ", clFiles.Select(f => f.name))}");
            return null;
        }
        TextAsset[] helperFiles = Array.FindAll(clFiles, s => s.name.Contains("Helper"));
        if (helperFiles.Length == 0)
        {
            Debug.LogError("Cannot find any helper functions in Resources/OpenCL_Scripts/. " +
                "All the files I have are: " + string.Join(", ", clFiles.Select(f => f.name)));
            return null;
        }
        TextAsset constantFile = Array.FindAll(clFiles, s => s.name.Contains("Constants"))[0];
        if (constantFile == null || constantFile.text.Length == 0)
        {
            Debug.LogError("Cannot find any constant in Resources/OpenCL_Scripts/. " +
                "All the files I have are: " + string.Join(", ", clFiles.Select(f => f.name)));
            return null;
        }

        string clSource = mainFile.text;
        foreach (TextAsset textAsset in helperFiles)
        {
            clSource = textAsset.text + "\n\n" + clSource;
        }
        clSource = constantFile.text + "\n\n" + clSource;

        return clSource;
    }


    public abstract List<Q> Update(List<Q> args, params object[] additionalParameters);

    protected abstract List<Q> SimplifyUpdateObjects(List<Q> args);
}
