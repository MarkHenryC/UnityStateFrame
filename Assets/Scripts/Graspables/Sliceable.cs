using UnityEngine;

namespace QS
{
    public class Sliceable : Placeable
    {
        public int minSlices, maxSlices;
        public GameObject sliceMarker, verticalSliceMarker;
        public Vector3 startSlicePos, endSlicePos, startChopPos, endChopPos;
        public GameObject[] markers;
        public GameObject[] verticalMarkers;
        public bool chop;

        private int currentSliceCount;
        private float sliceDistance; // for calculating cross-line spacing for chopping
        private float chopSpan; // start to end vertical lines

        public int SliceCount
        {
            get { return currentSliceCount; }
            set { currentSliceCount = value; }
        }

        public void SetStartSlicePos()
        {
            startSlicePos = sliceMarker.transform.localPosition;
        }

        public void SetEndSlicePos()
        {
            endSlicePos = sliceMarker.transform.localPosition;
        }

        public void SetStartChopPos()
        {
            startChopPos = verticalSliceMarker.transform.localPosition;
        }

        public void SetEndChopPos()
        {
            endChopPos = verticalSliceMarker.transform.localPosition;            
        }

        public void EditorMarkSlices()
        {
            Setup();
            MarkSlices(maxSlices);
        }

        public void MarkSlices(int slices)
        {
            currentSliceCount = slices;

            HideAll();

            if (slices > 0 && slices <= maxSlices)
            {
                chopSpan = Vector3.Distance(endChopPos, startChopPos);

                sliceMarker.transform.localPosition = startSlicePos;
                sliceMarker.SetActive(true);
                float t = 1f / slices;
                for (int i = 0; i < slices; i++)
                {
                    markers[i].transform.localPosition = Vector3.Lerp(startSlicePos, endSlicePos, (i + 1) * t);
                    markers[i].SetActive(true);
                }

                if (chop)
                {
                    if (markers.Length > 1)
                    {
                        sliceDistance = Vector3.Distance(startSlicePos, endSlicePos) / slices;
                        verticalSliceMarker.transform.localPosition = startChopPos;
                        verticalSliceMarker.SetActive(true);

                        int numLines = (int)(chopSpan / sliceDistance + 0.5);
                        Vector3 directionVec = (endChopPos - startChopPos).normalized;
                        
                        for (int i2 = 0; i2 < numLines; i2++)
                        {
                            verticalMarkers[i2].transform.localPosition = verticalSliceMarker.transform.localPosition + directionVec * ((i2 + 1) * sliceDistance);
                            verticalMarkers[i2].SetActive(true);
                        }
                    }
                    else
                        sliceDistance = 0f; // zero means don't use and there's no slicing
                }
            }
        }

        public void PresentForSlicing()
        {
            TransformToDest();
            Kinematic = true;
        }

        private void HideAll()
        {
            sliceMarker.SetActive(false);
            for (int i = 0; i < maxSlices; i++)
                if (markers[i])
                    markers[i].SetActive(false);

            if (chop)
            {
                verticalSliceMarker.SetActive(false);
                for (int i2 = 0; i2 < verticalMarkers.Length; i2++)
                    if (verticalMarkers[i2])
                        verticalMarkers[i2].SetActive(false);
            }
        }

        private void Setup()
        {
            if (currentSliceCount < minSlices)
                currentSliceCount = minSlices;
            markers = new GameObject[maxSlices];
            for (int i = 0; i < maxSlices; i++)
            {
                markers[i] = GameObject.Instantiate(sliceMarker, transform);
                markers[i].SetActive(false);
            }

            if (chop)
            {
                verticalMarkers = new GameObject[maxSlices];
                for (int i = 0; i < maxSlices; i++)
                {
                    verticalMarkers[i] = GameObject.Instantiate(verticalSliceMarker, transform);
                    verticalMarkers[i].SetActive(false);
                }
            }
        }
    }
}