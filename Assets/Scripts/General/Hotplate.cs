using UnityEngine;

namespace QS
{
    public class Hotplate : UnitFloatReceiver
    {
        public float heatValue = 0f;
        public ParticleSystem[] burner;
        public float maxValue = 2f;
        public float minValue = 0.05f;

        private Renderer hotplateRenderer;
        private Color hotplateColor = Color.red;

        private void Awake()
        {
            hotplateRenderer = GetComponent<Renderer>();

            SetValue(null, heatValue);
        }

        /// <summary>
        /// unitVal range 0.0 .. 1.0
        /// </summary>
        /// <param name="unitVal"></param>
        public override void SetValue(IUnitValueProvider p, float unitVal)
        {
            heatValue = unitVal;
            hotplateColor.r = heatValue;
            hotplateRenderer.sharedMaterial.color = hotplateColor;

            var newVal = unitVal * maxValue;
            foreach (var b in burner)
            {               
                var main = b.main;
                if (newVal < minValue && b.gameObject.activeSelf)
                    b.gameObject.SetActive(false);
                else if (!b.gameObject.activeSelf)
                    b.gameObject.SetActive(true);
                main.startLifetime = newVal;
            }
        }
    }

}