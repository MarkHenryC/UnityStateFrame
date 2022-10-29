using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Sd = System.Diagnostics;
using static Unity.Mathematics.math;

namespace QS
{
    public struct StructVec
    {
        public float x, y, z;

        public StructVec(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static StructVec Forward = new StructVec(0f, 0f, 1f);
        public static float Dot(StructVec v, StructVec w)
        {
            return v.x * w.x + v.y * w.y + v.z * w.z;
        }
    }

    public class ClassVec
    {
        public float x, y, z;

        public ClassVec(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static ClassVec Forward = new ClassVec(0f, 0f, 1f);
        public static float Dot(ClassVec v, ClassVec w)
        {
            return v.x * w.x + v.y * w.y + v.z * w.z;
        }
    }

    public class Tester : MonoBehaviour
    {
        public int Iterations = 100000;

        private ClassVec classVec = new ClassVec(0f, 0f, 0f);
        private StructVec structVec = new StructVec(0f, 0f, 0f);

        private Vector3 regularVec = Vector3.zero;

        private const float xCoeff = 123.45f;
        private const float yCoeff = 234.56f;
        private const float zCoeff = 345.67f;

        void Start()
        {
            Calc();
            ClassCalc();
            StructCalc();
            BurstCalc();
        }

        private void Calc()
        {
            int i;
            double accum = 0.0;

            Sd.Stopwatch s = Sd.Stopwatch.StartNew();

            s.Restart();
            for (i = 0; i < Iterations; i++)
            {
                regularVec.x = i * xCoeff;
                regularVec.y = i * yCoeff;
                regularVec.z = i * zCoeff;

                var temp = Vector3.Dot(regularVec, Vector3.forward);
                accum += Mathf.Sqrt(temp);
            }
            s.Stop();
            double elapsed1 = s.ElapsedMilliseconds / 1000f;
            double accum1 = accum;

            accum = 0.0;
            s.Restart();
            for (i = 0; i < Iterations; i++)
            {
                var v = new Vector3(i * xCoeff, i * yCoeff, i * zCoeff);
                var temp = Vector3.Dot(v, Vector3.forward);
                accum += Mathf.Sqrt(temp);
            }
            s.Stop();
            double elapsed2 = s.ElapsedMilliseconds / 1000f;

            Debug.Log($"Persistent Unity Vector time: {elapsed1}. Local Unity Vector time: {elapsed2}.\nFirst dot/sqrt value: {accum1}. Final dot/sqrt value: {accum}");
        }

        private void ClassCalc()
        {
            int i;
            double accum = 0.0;

            Sd.Stopwatch s = Sd.Stopwatch.StartNew();

            s.Restart();
            for (i = 0; i < Iterations; i++)
            {
                classVec.x = i * xCoeff;
                classVec.y = i * yCoeff;
                classVec.z = i * zCoeff;

                var temp = ClassVec.Dot(classVec, ClassVec.Forward);
                accum += Mathf.Sqrt(temp);
            }
            s.Stop();
            double elapsed1 = s.ElapsedMilliseconds / 1000f;
            double accum1 = accum;

            accum = 0.0;
            s.Restart();
            for (i = 0; i < Iterations; i++)
            {
                var temp = ClassVec.Dot(new ClassVec(i * xCoeff, i * yCoeff, i * zCoeff), ClassVec.Forward);
                accum += Mathf.Sqrt(temp);
            }
            s.Stop();
            double elapsed2 = s.ElapsedMilliseconds / 1000f;

            Debug.Log($"Persistent Class time: {elapsed1}. Local Class time: {elapsed2}.\nFirst dot/sqrt value: {accum1}. Final dot/sqrt value: {accum}");
        }

        private void StructCalc()
        {
            int i;
            double accum = 0.0;

            Sd.Stopwatch s = Sd.Stopwatch.StartNew();

            s.Restart();
            for (i = 0; i < Iterations; i++)
            {
                structVec.x = i * xCoeff;
                structVec.y = i * yCoeff;
                structVec.z = i * zCoeff;

                var temp = StructVec.Dot(structVec, StructVec.Forward);
                accum += Mathf.Sqrt(temp);
            }
            s.Stop();
            double elapsed1 = s.ElapsedMilliseconds / 1000f;
            double accum1 = accum;

            accum = 0.0;
            s.Restart();
            for (i = 0; i < Iterations; i++)
            {
                var temp = StructVec.Dot(new StructVec(i * xCoeff, i * yCoeff, i * zCoeff), StructVec.Forward);
                accum += Mathf.Sqrt(temp);
            }
            s.Stop();
            double elapsed2 = s.ElapsedMilliseconds / 1000f;

            Debug.Log($"Persistent Struct time: {elapsed1}. Local Struct time: {elapsed2}.\nFirst dot/sqrt value: {accum1}. Final dot/sqrt value: {accum}");
        }

        private void BurstCalc()
        {
            var accumData = new NativeArray<double>(1, Allocator.TempJob);

            var burstJob = new TestJobPersistent
            {
                iterations = Iterations,
                Accumulator = accumData
            };

            JobHandle jobHandle1 = burstJob.Schedule();

            Sd.Stopwatch sw = Sd.Stopwatch.StartNew();
            sw.Restart();

            jobHandle1.Complete();

            sw.Stop();

            Debug.Log($"Burst: Persistent vector time: {sw.ElapsedMilliseconds / 1000f}.\nFinal dot/sqrt value: {accumData[0]}");

            var burstJob2 = new TestJobLocal
            {
                iterations = Iterations,
                Accumulator = accumData
            };

            JobHandle jobHandle2 = burstJob2.Schedule();

            sw.Restart();

            jobHandle2.Complete();

            sw.Stop();

            Debug.Log($"Burst: Local vector time: {sw.ElapsedMilliseconds / 1000f}.\nFinal dot/sqrt value: {accumData[0]}");

            accumData.Dispose();
        }

        [BurstCompile(CompileSynchronously = true)]
        public struct TestJobPersistent : IJob
        {
            [ReadOnly]
            public float iterations;

            [WriteOnly]
            public NativeArray<double> Accumulator;

            public Vector3 regularVec;

            private const float xCoeff = 123.45f;
            private const float yCoeff = 234.56f;
            private const float zCoeff = 345.67f;

            public void Execute()
            {
                double accum = 0f;

                for (int i = 0; i < iterations; i++)
                {
                    regularVec.x = i * xCoeff;
                    regularVec.y = i * yCoeff;
                    regularVec.z = i * zCoeff;

                    var temp = Vector3.Dot(regularVec, Vector3.forward);
                    accum += Mathf.Sqrt(temp);
                }

                Accumulator[0] = accum;
            }
        }

        [BurstCompile(CompileSynchronously = true)]
        public struct TestJobLocal : IJob
        {
            [ReadOnly]
            public float iterations;

            [WriteOnly]
            public NativeArray<double> Accumulator;

            public void Execute()
            {
                double accum = 0f;

                for (int i = 0; i < iterations; i++)
                {
                    var temp = Vector3.Dot(new Vector3(i * xCoeff, i * yCoeff, i * zCoeff), Vector3.forward);
                    accum += Mathf.Sqrt(temp);
                }

                Accumulator[0] = accum;
            }
        }
    }
}