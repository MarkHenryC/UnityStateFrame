using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Jobs;
using Sd = System.Diagnostics;
using static Unity.Mathematics.math;
using Unity.Collections;
using Unity.Burst;

namespace QS
{
	public class ParallelJob : MonoBehaviour 
	{
		void Awake () 
		{
			
		}
		
		void Start () 
		{
			
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

                }

                Accumulator[0] = accum;
            }
        }
    }
}