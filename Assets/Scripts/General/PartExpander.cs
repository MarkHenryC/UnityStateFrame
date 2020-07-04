using UnityEngine;

namespace QS
{
    /// <summary>
    /// Parts will be expanded outward spherically
    /// from where they are in relation to the 
    /// centre of this parent object
    /// </summary>
	public class PartExpander : MonoBehaviour
    {
        public Transform[] parts;
        public AnimationCurve expansionPath;

        private Vector3[] partDirections, partStartPositions;
        private ClickableModel[] clickables;
        private ClickableModel currentClicked, previousClicked, queuedClick;

        public ClickableModel CurrentClicked { private set { currentClicked = value; } get { return currentClicked; } }
        public ClickableModel[] Clickables { get { return clickables; } }

        private void Awake()
        {
            partDirections = new Vector3[parts.Length];
            partStartPositions = new Vector3[parts.Length];
            clickables = new ClickableModel[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                partStartPositions[i] = (parts[i].position - transform.position);
                partDirections[i] = partStartPositions[i].normalized;
                clickables[i] = parts[i].GetComponent<ClickableModel>();
                if (clickables[i])
                    clickables[i].OnClick = Clicked;
            }
        }

        private void Update()
        {
            float t = 0f;

            for (int i = 0; i < parts.Length; i++)
            {
                if (currentClicked && parts[i] == currentClicked.transform)
                {
                    t = expansionPath.Evaluate(currentClicked.FloatStore);
                    currentClicked.FloatStore += Time.deltaTime;
                    parts[i].position = transform.TransformPoint(partStartPositions[i] + partDirections[i] * t);
                }

                if (previousClicked && parts[i] == previousClicked.transform)
                {
                    if (PrettyCloseToZero(t))
                    {
                        previousClicked = null;
                        if (queuedClick)
                        {
                            previousClicked = currentClicked;
                            currentClicked = queuedClick;
                            currentClicked.FloatStore = 0f;
                            queuedClick = null;
                        }
                        parts[i].position = transform.TransformPoint(partStartPositions[i]);
                    }
                    else
                    {
                        t = expansionPath.Evaluate(previousClicked.FloatStore);
                        parts[i].position = transform.TransformPoint(partStartPositions[i] + partDirections[i] * t);
                    }
                }
            }
        }

        private void Clicked(ClickableModel m)
        {
            if (previousClicked)
            {
                queuedClick = m;
            }
            else
            {
                previousClicked = currentClicked;
                currentClicked = m;
                currentClicked.FloatStore = 0f;
            }
        }

        private bool PrettyCloseToZero(float f)
        {
            return (Mathf.Abs(f) < .0009f);
        }
    }
}