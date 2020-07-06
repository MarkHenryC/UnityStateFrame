using UnityEngine;

namespace QS
{
    public class HoldingBeam : PointerFeedback
    {
        private LineRenderer lineRenderer;
        private Vector3 initialReticleScale;

        public override void Awake()
        {
            base.Awake();

            if (reticle)
                initialReticleScale = reticle.transform.localScale;

            lineRenderer = GetComponent<LineRenderer>();
            if (!lineRenderer)
                lineRenderer = gameObject.AddComponent<LineRenderer>();

            DrawLine(Vector3.zero, Vector3.zero); // clear visible beam
        }

        /// <summary>
        /// Need to switch on before use. Defaults to 
        /// inactive on startup
        /// </summary>
        /// <param name="activate"></param>
        public override void Activate(bool activate)
        {
            if (controllerRepresentation)
                controllerRepresentation.gameObject.SetActive(true);
            if (reticle)
                reticle.SetActive(activate);
            if (lineRenderer)
                DrawLine(Vector3.zero, Vector3.zero);
            isActive = activate;
        }

        public override void SetControllerOrientation(Vector3 pos, Quaternion rotation)
        {
            base.SetControllerOrientation(pos, rotation);

            if (controllerRepresentation)
            {
                controllerRepresentation.position = pointerPos;
                controllerRepresentation.localRotation = pointerRot;
            }
        }

        public override void OnFrame(VrEventInfo processedVrEventInfo)
        {
            SetControllerOrientation(processedVrEventInfo.ControllerPosition, processedVrEventInfo.ControllerRotation);
            SetReticlePosition(processedVrEventInfo.HoldingTargetPosition);

            AlignReticleToCamera();

            Draw();
        }

        public override void Draw(bool showBeam = true, bool showReticle = true)
        {
            if (showBeam)
                DrawLine(pointerPos, pointerTarget);
            else
                DrawLine(Vector3.zero, Vector3.zero); // clear visible beam

            if (showReticle && reticle)
            {
                reticle.SetActive(true);
                reticle.transform.position = pointerTarget;
            }
            else if (reticle && reticle.activeSelf)
                reticle.SetActive(false);
        }

        private void DrawLine(Vector3 startPoint, Vector3 endPoint)
        {
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                float t = (float)i / lineRenderer.positionCount;
                Vector3 pos = Vector3.Lerp(startPoint, endPoint, t);
                lineRenderer.SetPosition(i, pos);
            }
        }

        private void ScaleReticle()
        {
            Plane plane = new Plane(mainCam.transform.forward, mainCam.transform.position);
            float dist = plane.GetDistanceToPoint(reticle.transform.position);
            if (reticle)
                reticle.transform.localScale = initialReticleScale * dist * reticleScale;
        }
    }
}