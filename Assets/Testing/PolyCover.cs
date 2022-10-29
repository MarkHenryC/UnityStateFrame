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
    public struct Quad
    {
        public Vector3 BottomLeft, TopLeft, TopRight, BottomRight;
        public int IxBottomLeft, IxTopLeft, IxTopRight, IxBottomRight;
    }

    /// <summary>
    /// Some test appearances fx for elevator.
    /// Could be done on a much more fine-grained   
    /// level to simulate a fade-in. A burst
    /// batch process is ideal for this if
    /// better performance is needed.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class PolyCover : MonoBehaviour
    {
        public float Width;
        public float Height;
        public int Columns;
        public int Rows;
        public float DissolveTime = 3f;
        [Range(0f, 1f)]
        public float DissolveAmount;

        private Quad[,] quadGrid;
        private Vector3[] vertices;

        private NativeArray<float> shrinkAmt;
        private NativeArray<Vector3> originalVertices;
        private NativeArray<Vector3> modifiedVertices;
        private ProcessingJob burstJob;
        private JobHandle burstJobHandle;
        private bool isBatching;

        private Mesh mesh;
        private MeshRenderer meshRenderer;

        private const int BottomLeft = 0, TopLeft = 1, TopRight = 1, BottomRight = 3;

        private void Deallocate()
        {
            meshRenderer.enabled = false;
            shrinkAmt.Dispose();
            originalVertices.Dispose();
            modifiedVertices.Dispose();
        }

        public void Setup()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.enabled = true;

            quadGrid = new Quad[Rows, Columns];

            GetComponent<MeshFilter>().mesh = mesh = new Mesh();
            mesh.MarkDynamic();
            mesh.name = "Thingo";

            int vertexPositions = quadGrid.Length * 4;
            vertices = new Vector3[vertexPositions];

            CalcQuads();
            CalcVertices();

            shrinkAmt = new NativeArray<float>(3, Allocator.Persistent);
            // Cache quad centre point
            shrinkAmt[1] = (Width / Columns) / 2f;
            shrinkAmt[2] = (Height / Rows) / 2f;

            originalVertices = new NativeArray<Vector3>(mesh.vertices, Allocator.Persistent);
            modifiedVertices = new NativeArray<Vector3>(mesh.vertices.Length, Allocator.Persistent);
        }

        public void Dissolve()
        {
            Setup();
            StartCoroutine(CoShrinkBy(DissolveAmount, DissolveTime, Deallocate));
        }

        public void BatchDissolve()
        {
            Setup();
            StartCoroutine(CoShrinkBatch(DissolveAmount, DissolveTime, Deallocate));
        }

        public void Restore()
        {
            Shrink(0f);
        }

        /// <summary>
        /// Just for testing in editor
        /// </summary>
        public void ShrinkTo()
        {
            Setup();
            Shrink(DissolveAmount);
            Deallocate();
        }

        private void Shrink(float amount)
        {
            for (int quadRow = 0; quadRow < Rows; quadRow++)
            {
                for (int quadCol = 0; quadCol < Columns; quadCol++)
                {
                    var quad = quadGrid[quadRow, quadCol];

                    float midX = (quad.TopRight.x - quad.TopLeft.x) / 2f;
                    float midY = (quad.TopLeft.y - quad.BottomLeft.y) / 2f;

                    float scaledX = midX * amount;
                    float scaledY = midY * amount;

                    vertices[quad.IxBottomLeft].x = quad.BottomLeft.x + scaledX;
                    vertices[quad.IxBottomLeft].y = quad.BottomLeft.y + scaledY;
                    vertices[quad.IxTopLeft].x = quad.TopLeft.x + scaledX;
                    vertices[quad.IxTopLeft].y = quad.TopLeft.y - scaledY;
                    vertices[quad.IxTopRight].x = quad.TopRight.x - scaledX;
                    vertices[quad.IxTopRight].y = quad.TopRight.y - scaledY;
                    vertices[quad.IxBottomRight].x = quad.BottomRight.x - scaledX;
                    vertices[quad.IxBottomRight].y = quad.BottomRight.y + scaledY;
                }
            }

            mesh.SetVertices(vertices);
        }

        void CalcQuads()
        {
            float colWidth = Width / Columns;
            float rowHeight = Height / Rows;

            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    float leftX = (col * colWidth);
                    float botY = row * rowHeight;
                    float rightX = leftX + colWidth;
                    float topY = botY + rowHeight;

                    quadGrid[row, col] = new Quad
                    {
                        BottomLeft = new Vector3(leftX, botY, 0f),
                        TopLeft = new Vector3(leftX, topY, 0f),
                        TopRight = new Vector3(rightX, topY, 0f),
                        BottomRight = new Vector3(rightX, botY, 0f)
                    };
                }
            }
        }

        void CalcVertices()
        {
            int[] triangles = new int[vertices.Length * 6];
            int vi = 0;
            int ti = 0;

            for (int quadRow = 0; quadRow < Rows; quadRow++)
            {
                for (int quadCol = 0; quadCol < Columns; quadCol++)
                {
                    var quad = quadGrid[quadRow, quadCol];

                    triangles[ti++] = vi;
                    vertices[vi] = quad.BottomLeft;
                    quad.IxBottomLeft = vi;

                    vi++;
                    triangles[ti++] = vi;
                    vertices[vi] = quad.TopLeft;
                    quad.IxTopLeft = vi;

                    vi++;
                    triangles[ti++] = vi;
                    vertices[vi] = quad.TopRight;
                    quad.IxTopRight = vi;

                    vi++;
                    triangles[ti++] = vi;
                    vertices[vi] = quad.BottomRight;
                    quad.IxBottomRight = vi;

                    quadGrid[quadRow, quadCol] = quad;

                    triangles[ti++] = quad.IxBottomLeft;
                    triangles[ti++] = quad.IxTopRight;

                    vi++;
                }
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
        }

        private IEnumerator CoShrinkBy(float amount = 1f, float time = 3f, 
            Action OnComplete = null)
        {
            float counter = 0f;

            while (counter < time)
            {
                float t = counter / time;

                Shrink(t * amount);

                counter += Time.deltaTime;

                yield return null;
            }

            OnComplete?.Invoke();
        }

        private IEnumerator CoShrinkBatch(float amount = 1f, float time = 3f,
            Action OnComplete = null)
        {
            float counter = 0f;

            while (counter < time)
            {
                if (!isBatching)
                {
                    isBatching = true;

                    float t = counter / time;
                    shrinkAmt[0] = t * amount;

                    burstJob = new ProcessingJob
                    {
                        OriginalVertices = originalVertices,
                        ModifiedVertices = modifiedVertices,
                        ShrinkAmt = shrinkAmt
                    };

                    burstJobHandle = burstJob.Schedule(originalVertices.Length, 64);

                    yield return null;

                    burstJobHandle.Complete();

                    mesh.SetVertices(burstJob.ModifiedVertices);

                    isBatching = false;
                }

                counter += Time.deltaTime;

                yield return null;
            }

            OnComplete?.Invoke();
        }


        //[BurstCompile(CompileSynchronously = true)]
        //[BurstCompile]
        public struct ProcessingJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<float> ShrinkAmt;

            [ReadOnly]
            public NativeArray<Vector3> OriginalVertices;

            [WriteOnly]
            public NativeArray<Vector3> ModifiedVertices;

            public void Execute(int vi)
            {
                int ix = vi % 4;
                Vector3 curVertex = OriginalVertices[vi];

                float scaledX = ShrinkAmt[1] * ShrinkAmt[0];
                float scaledY = ShrinkAmt[2] * ShrinkAmt[0];

                switch (ix)
                {
                    case 0: // BottomLeft
                        curVertex.x += scaledX;
                        curVertex.y += scaledY;
                        break;
                    case 1: // TopLeft
                        curVertex.x += scaledX;
                        curVertex.y -= scaledY;
                        break;
                    case 2: // TopRight
                        curVertex.x -= scaledX;
                        curVertex.y -= scaledY;
                        break;
                    case 3: // BottomRight
                        curVertex.x -= scaledX;
                        curVertex.y += scaledY;
                        break;
                }

                ModifiedVertices[vi] = curVertex;
            }
        }
    }
}