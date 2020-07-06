using System;
using UnityEngine;

namespace QS
{
    [RequireComponent(typeof(LineRenderer))]
    public class Connection : MonoBehaviour
    {
        public Block container;
        public Connection to;
        public int iterations = 4; // Note even numbers are quicker as no need to copy buffer        
        public Vector3 PinPosition { get; private set; }
        public Connection From { get; set; }
        public LayerMask receiverHit;
        public float cableWidth;
        public float Sag = .35f;

        public Func<Connection, Connection> Next;
        public Action<Connection> OnConnectionChanged;

        private Connectable outPin;
        private Receivable inTab;
        private LineRenderer connectionLine;
        private bool draggingPin;

        private Vector3[] controlPoints;
        private Vector3[] points;
        private int pointCount;
        private Vector3[] buffer; // backbuffer

        /// <summary>
        /// For evaluating the circuit
        /// </summary>
        /// <returns></returns>
        public virtual Connection GetNext()
        {
            // Next() is set in parent Block component
            // and behaves according to type and state
            // of block, e.g switch, light etc

            if (Next != null)
                return Next(this);
            else
                return to ?? From;
        }

        private void Awake()
        {
            container = GetComponentInParent<Block>();
            container.Bind(this);

            outPin = GetComponentInChildren<Connectable>();
            Debug.Assert(outPin, "No Connectable component");

            inTab = GetComponentInChildren<Receivable>();
            Debug.Assert(inTab, "No Receivable component");

            outPin.CallOnClick = CallOnClick;
            outPin.CallOnRelease = CallOnRelease;
            outPin.CallOnDrop = CallOnDrop;

            PinPosition = outPin.transform.position;

            connectionLine = GetComponent<LineRenderer>();

            controlPoints = new Vector3[3];
            pointCount = CalcPointsForIterations(iterations);
            points = new Vector3[pointCount];
            buffer = new Vector3[pointCount];
            connectionLine.positionCount = pointCount;
            if (cableWidth != 0f)
                connectionLine.startWidth = connectionLine.endWidth = cableWidth;
        }

        private void CallOnClick(Connectable c)
        {
            Debug.Assert(!(From && to), "Only From or to should be set");

            if (From)
            {
                From.ClearOutgoingConnection();
                From = null;
            }
            else if (to)
            {
                to.From = null;
            }

            ClearLine();
            draggingPin = true;
        }

        private void CallOnRelease(Connectable c)
        {
            draggingPin = false;
            OnConnectionChanged?.Invoke(this);
        }

        private void Update()
        {
            if (draggingPin)
            {
                if (Physics.Raycast(ControllerInput.Instance.ControllerPosition,
                    ControllerInput.Instance.ControllerDirection, out RaycastHit hitInfo, 10f, receiverHit))
                {
                    outPin.transform.position = hitInfo.point;
                }
                DrawLine(PinPosition, outPin.transform.position);
            }
        }

        /// <summary>
        /// We've asked our outgoing connection pin
        /// to notify us when it's dropped. We confirm
        /// it's a Connectable (which should be a given)
        /// then check if it's dropped on a ConnectionReceiver.
        /// If so, clear any existing connection from either
        /// terminal and form a new connection. If it's 
        /// dropped outside of a receiver proximity, 
        /// clear the outgoing connection if any and
        /// remove connection line.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="r"></param>
        /// <param name="inProximity"></param>
        private void CallOnDrop(Placeable p, Receivable r, bool inProximity)
        {
            Connectable conn = p as Connectable;
            if (conn)
            {
                Debug.Log("Dropped connector");

                ConnectionReceiver receiver = r as ConnectionReceiver;

                if (receiver && inProximity)
                    SetConnection(receiver.connection);
                else
                    ClearOutgoingConnection();
            }

            p.ReturnToInitialPosition();
        }

        private void SetConnection(Connection dest)
        {
            if (dest && dest != to && dest != this)
            {
                dest.ClearOutgoingConnection();
                dest.ClearBackConnection();

                if (to)
                    to.From = null;

                to = dest;

                to.From = this; // Track connection line if it later needs clearing

                DrawLine(PinPosition, to.PinPosition);

                container.UpdateConnection(Circuit.TraceTrigger.Rewire);

                Debug.LogFormat("Connection set to {0}", dest.name);
            }
            else
                ClearOutgoingConnection();
        }

        public void ClearOutgoingConnection()
        {
            Debug.Assert(!(From && to), "ClearOutgoingConnection: only From or to should be set");

            if (to)
                to.From = null;

            to = null;

            ClearLine();

            container.UpdateConnection(Circuit.TraceTrigger.Rewire);
        }

        private void ClearBackConnection()
        {
            if (From)
                From.ClearOutgoingConnection();
        }

        private void DrawLine(Vector3 a, Vector3 b)
        {
            Vector3 mid = Vector3.Lerp(a, b, .5f);
            mid.y = Mathf.Min(a.y, b.y) - Sag;
            controlPoints[0] = a;
            controlPoints[1] = mid;
            controlPoints[2] = b;

            ChaikinsCurveBuffered();
            connectionLine.SetPositions(points);
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

            var input = points;
            var output = buffer;

            for (int i = 0; i < iterations; i++)
            {
                dataSize = ApplyChaikins(input, dataSize, output);
                var temp = input;
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

            for (var i = 0; i < dataSize - 1; i++)
            {
                var p0 = path[i];
                var p1 = path[i + 1];
                var q = (p0 * .75f) + (p1 * .25f);
                var r = (p0 * .25f) + (p1 * .75f);

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

            for (var i = 0; i < dataSize - 1; i++)
                index += 2;

            return index;
        }
    }
}