using Silk.NET.OpenCL;
using System;
using System.IO;
using UnityEngine;

public class OpenClRunner<T>
{
    protected nint context;
    protected nint commandQueue;
    protected nint program;
    protected nint kernel;
    protected CL cl;

    public OpenClRunner(string filePath, string functionName)
    {
        cl = CL.GetApi();
        nint device = 0;

        context = CreateContext(cl);
        commandQueue = CreateCommandQueue(cl, context, ref device);
        program = CreateProgram(cl, context, device, filePath);
        kernel = CreateKernel(cl, program, functionName);
    }

    protected bool Run(nuint[] globalWorkSize, nuint[] localWorkSize, int resultSize, nint[] memObjects, out T[] result)
    {
        cl = OpenClInterfaceImplementation.Enqueue(cl, commandQueue, kernel, globalWorkSize, localWorkSize, memObjects);
        cl = OpenClInterfaceImplementation.ReadBuffer(cl, commandQueue, memObjects, out result, resultSize);
        return true;
    }

    public static unsafe nint CreateKernel(CL cl, nint program, string functionName)
    {
        nint kernel = cl.CreateKernel(program, functionName, null);
        if (kernel == IntPtr.Zero)
        {
            Debug.Log("Failed to create kernel");
        }
        return kernel;
    }

    public static unsafe nint CreateCommandQueue(CL cl, nint context, ref nint device)
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

    public static unsafe nint CreateContext(CL cl)
    {
        var errNum = cl.GetPlatformIDs(1, out nint firstPlatformId, out uint numPlatforms);
        if (errNum != (int)ErrorCodes.Success || numPlatforms <= 0)
        {
            Debug.Log("Failed to find any OpenCL platforms.");
            return IntPtr.Zero;
        }

        // Next, create an OpenCL context on the platform.  Attempt to
        // create a GPU-based context, and if that fails, try to create
        // a CPU-based context.
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
    public static unsafe nint CreateProgram(CL cl, nint context, nint device, string fileName)
    {
        TextAsset[] clFiles = Resources.LoadAll<TextAsset>("OpenCL_Scripts");
        string clSource = Array.FindAll(clFiles, match: s => s.name.Equals(fileName))[0].text;

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
}
