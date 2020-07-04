using UnityEngine;

namespace QS
{
    /// <summary>
    /// A handy way of lining up an object in-scene
    /// (just drag in the target transform). Can be
    /// done in editor or at runtime if alignOnStart selected
    /// </summary>
    public class AlignTransform : MonoBehaviour
    {
        public Transform alignWith;
        public bool alignOnStart;
        [Tooltip("By default we only use x & z")]
        public bool includeY;
        public bool runtimeSave;

        public void Align()
        {
            if (alignWith)
            {
                if (includeY)
                {
                    ActivitySettings.Asset.cachedPosition = transform.position = alignWith.position;
                }
                else
                {
                    Vector3 curPos = transform.position;
                    Vector3 newPos = alignWith.position;
                    newPos.y = curPos.y;
                    ActivitySettings.Asset.cachedPosition = transform.position = newPos;
                }

                ActivitySettings.Asset.cachedRotation = transform.rotation = alignWith.rotation;
            }
        }

        public void LoadSavedPosition()
        {
            transform.position = ActivitySettings.Asset.cachedPosition;
            transform.rotation = ActivitySettings.Asset.cachedRotation;
        }

        private void Start()
        {
            if (alignOnStart)
                Align();
        }
    }

}