using System.IO;
using TMPro;
using UnityEngine;

namespace QS
{
    public class ProjectLoader : MonoBehaviour
    {
        public TextMeshPro tmp;

        private void Awake()
        {
            Directory.CreateDirectory(Utils.VIDEO_PATH);
            tmp.text = "Version: " + Application.version;

            ActivitySettings.Asset.ResetRaycastDistance();

            Overrides.LoadOverrides(); // Either load values from json or set to defaults
        }
    }
}
