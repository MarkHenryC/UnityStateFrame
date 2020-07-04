using System;
using UnityEngine;

namespace QS
{
    [RequireComponent(typeof(LineRenderer))]
    public class Cable : MonoBehaviour
    {
        public Transform objectFrom, objectTo; // For static connectors or connecting objects
        public bool straight;
        public int iterations = 4; // Note even numbers are quicker as no need to copy buffer
        public float sag = .35f;

        private LineRenderer connectionLine;
        private Vector3[] controlPoints;
        private Vector3[] points;
        private int pointCount;
        private Vector3[] buffer; // backbuffer

        private Vector3 pointFrom, pointTo;

        private void Awake()
        {
            connectionLine = GetComponent<LineRenderer>();
            if (straight)
            {
                pointCount = 2;
            }
            else
            {
                controlPoints = new Vector3[3];
                pointCount = CalcPointsForIterations(iterations);
                points = new Vector3[pointCount];
                buffer = new Vector3[pointCount];
            }

            connectionLine.positionCount = pointCount;

            ClearLine();
            UpdateConnection();
        }

        public void UpdateConnection()
        {
            if (objectFrom && objectTo)
                UpdateConnection(objectFrom.position, objectTo.position);
        }

        public void UpdateConnection(Vector3 to)
        {
            if (objectFrom)
                UpdateConnection(objectFrom.position, to);
        }

        public void UpdateConnection(Vector3 from, Vector3 to)
        {
            DrawLine(from, to);
        }

        private void DrawLine(Vector3 a, Vector3 b)
        {
            if (straight)
            {
                connectionLine.SetPosition(0, a);
                connectionLine.SetPosition(1, b);
            }
            else
            {
                Vector3 mid = Vector3.Lerp(a, b, .5f);
                mid.y = Mathf.Min(a.y, b.y) - sag;
                controlPoints[0] = a;
                controlPoints[1] = mid;
                controlPoints[2] = b;

                ChaikinsCurveBuffered();
                connectionLine.SetPositions(points);
            }
        }

        private void ClearLine()
        {
            //DrawLine(Vector3.zero, Vector3.zero);
            for (int i = 0; i < connectionLine.positionCount; i++)
                connectionLine.SetPosition(i, Vector3.zero);
        }

        private void ChaikinsCurveBuffered()
        {
            Array.Copy(controlPoints, points, controlPoints.Length);

            int dataSize = controlPoints.Length;

            Vector3[] input = points;
            Vector3[] output = buffer;

            for (int i = 0; i < iterations; i++)
            {
                dataSize = ApplyChaikins(input, dataSize, output);
                Vector3[] temp = input;
                input = output;
                output = temp;
            }

            if (points != input)
                Array.Copy(input, points, pointCount);
        }

        private int ApplyChaikins(Vector3[] path, int dataSize, Vector3[] output)
        {
            int index = 0;
            output[index++] = path[0];

            for (int i = 0; i < dataSize - 1; i++)
            {
                Vector3 p0 = path[i];
                Vector3 p1 = path[i + 1];
                Vector3 q = (p0 * .75f) + (p1 * .25f);
                Vector3 r = (p0 * .25f) + (p1 * .75f);

                output[index++] = q;
                output[index++] = r;
            }

            output[index++] = path[dataSize - 1];

            return index;
        }

        private int CalcPointsForIterations(int iterations)
        {
            int dataSize = controlPoints.Length;

            for (int i = 0; i < iterations; i++)
                dataSize = CalcDataSize(dataSize);

            return dataSize;
        }

        private int CalcDataSize(int dataSize)
        {
            int index = 2;

            for (int i = 0; i < dataSize - 1; i++)
                index += 2;

            return index;
        }

        private void SetControlPoints()
        {
            for (int i = 0; i < controlPoints.Length; i++)
            {
                controlPoints[i].x += (UnityEngine.Random.value - .5f) / 100f;
                controlPoints[i].y += (UnityEngine.Random.value - .5f) / 100f;
                controlPoints[i].z += (UnityEngine.Random.value - .5f) / 100f;
            }
        }

    }
}