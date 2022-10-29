using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Sd = System.Diagnostics;

public class Tester2 : MonoBehaviour
{
    public int Iterations = 100000000;

    // Create a native array of a single float to store the result. Using a 
    // NativeArray is the only way you can get the results of the job, whether
    // you're getting one value or an array of values.
    NativeArray<double> result;
    // Create a JobHandle for the job
    JobHandle handle;

    private bool jobRun;

    private const float xCoeff = 123.45f;
    private const float yCoeff = 234.56f;
    private const float zCoeff = 345.67f;

    // Set up the job
    public struct MyJob : IJob
    {
        public float xCoeff, yCoeff, zCoeff;
        public int iterations;
        public NativeArray<double> result;

        public void Execute()
        {
            float accum = 0f;

            for (int i = 0; i < iterations; i++)
            {
                var temp = Vector3.Dot(new Vector3(i * xCoeff, i * yCoeff, i * zCoeff), Vector3.forward);
                accum += Mathf.Sqrt(temp);
            }

            result[0] = accum;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (jobRun)
            return;

        // Set up the job data
        result = new NativeArray<double>(1, Allocator.TempJob);

        MyJob jobData = new MyJob
        {
            xCoeff = xCoeff,
            yCoeff = yCoeff,
            zCoeff = zCoeff,
            iterations = Iterations,
            result = result
        };

        // Schedule the job
        handle = jobData.Schedule();
    }

    private void LateUpdate()
    {
        if (jobRun)
            return;

        jobRun = true;

        Sd.Stopwatch sw = Sd.Stopwatch.StartNew();
        sw.Start();

        // Sometime later in the frame, wait for the job to complete before accessing the results.
        handle.Complete();

        sw.Stop();

        Debug.Log($"Burst: Persistent local vector time: {sw.ElapsedMilliseconds / 1000f}.\nFinal dot/sqrt value: {result[0]}");

        //// All copies of the NativeArray point to the same memory, you can access the result in "your" copy of the NativeArray
        //double d = result[0];
        //Debug.Log($"Result: {d}");

        // Free the memory allocated by the result array
        result.Dispose();
    }


}