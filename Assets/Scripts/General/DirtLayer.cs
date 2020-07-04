using UnityEngine;

namespace QS
{
    public class DirtLayer : MonoBehaviour
    {
        public Texture2D dirtTexture;
        public int width = 256, height = 256;
        public int mopWidth = 8, mopHeight = 8;
        public bool generate;
        public float updateInterval = .1f; // 10 per second
        public int probability = 9;

        private Color32[] pixels;
        private int totalPixels;
        private Texture2D texture;
        private Color32[] mopPixels;
        private int halfMopWidth, halfMopHeight;
        private float pixelApplyCounter;
        private float startDirt;
        private float endDirt;

        public void Mop(float uvx, float uvy)
        {
            CleanArea((int)((width - 1) * uvx), (int)((height - 1) * uvy));
        }

        public void Show(bool show)
        {
            gameObject.SetActive(show);
        }

        public int GetDirt()
        {
            Color32[] pixels = texture.GetPixels32();
            int transAccum = 0;
            foreach (Color32 col in pixels)
                transAccum += col.a;
            return transAccum;
        }

        public void Initialize()
        {
            MakeDirty(generate);

            Renderer renderer = GetComponent<Renderer>();
            renderer.sharedMaterial.mainTexture = texture;
            mopPixels = new Color32[mopWidth * mopHeight];
            halfMopWidth = mopWidth / 2;
            halfMopHeight = mopHeight / 2;

            SetMopColor(Color.clear);
        }

        public int PercentRemoved()
        {
            endDirt = GetDirt();
            return (int)((1.0f - endDirt / startDirt) * 100f);
        }

        public void MakeDirty(bool random, bool circular = false)
        {
            if (random)
                RandomFill(circular);
            else
                pixels = dirtTexture.GetPixels32();

            texture.SetPixels32(pixels);
            texture.Apply();

            startDirt = GetDirt();
        }

        private void Awake()
        {
            totalPixels = width * height;
            texture = new Texture2D(width, height);
            pixels = new Color32[totalPixels];

            Initialize();
        }

        private void SetMopColor(Color32 col)
        {
            for (int i = 0; i < mopPixels.Length; i++)
                mopPixels[i] = col;
        }

        private void RandomFill(bool circular = false)
        {
            if (circular)
                CircularFill();
            else
                for (int i = 0; i < totalPixels; i++)
                {
                    if (i % Random.Range(1, probability) == 0)
                        pixels[i] = Color.black;
                    else
                        pixels[i] = Color.clear;
                }
        }

        private void CircularFill()
        {
            int measure = Mathf.Min(width, height);
            int mid = measure >> 1;
            float radSq = mid * mid;
            int iX = 0, iY = 0;
            if (width > height)
                iX = (width - height) >> 1;
            else if (height > width)
                iY = height - width >> 1;

            int i = 0;
            for (int iy = iY; iy < width; iy++)
                for (int ix = iX; ix < height; ix++)
                {
                    int dx = mid - ix;
                    int dy = mid - iy;
                    if ((dx * dx + dy * dy) <= radSq && i % Random.Range(1, probability) == 0)
                        pixels[i] = Color.black;
                    else
                        pixels[i] = Color.clear;

                    i++;
                }
        }

        private void CleanArea(int x, int y)
        {
            int startX = Mathf.Max(x - halfMopWidth, 0);
            int endX = Mathf.Min(x + halfMopWidth, width);
            int startY = Mathf.Max(y - halfMopHeight, 0);
            int endY = Mathf.Min(y + halfMopHeight, height);
            int modWidth = endX - startX;
            int modHeight = endY - startY;
            pixelApplyCounter += Time.deltaTime;

            for (int cy = startY; cy <= endY; cy++)
                for (int cx = startX; cx <= endX; cx++)
                    texture.SetPixels32(startX, startY, modWidth, modHeight, mopPixels);

            if (pixelApplyCounter >= updateInterval)
            {
                texture.Apply();
                pixelApplyCounter = 0f;
            }
        }
    }
}