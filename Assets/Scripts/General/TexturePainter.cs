using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace QS
{
    public class TexturePainter : MonoBehaviour
    {
        public Texture2D sourceTexture;
        public Material targetMaterial;
        public int brushWidth = 8, brushHeight = 8;
        public float updateInterval = .1f; // 10 per second
        public Texture2D sourceReplacementTexture;

        private Color32[] brushPixels;
        private int halfBrushWidth, halfBrushHeight;
        private float pixelApplyCounter;
        private Texture2D targetTexture, replacementTexture;
        private Dictionary<Color32, int> paintRecorder;

        private void OnEnable()
        {
            paintRecorder = new Dictionary<Color32, int>();
        }

        private void OnDisable()
        {
            paintRecorder.Clear();
        }

        public Dictionary<Color32, int> PaintRecorder => paintRecorder;

        public void Initialize(Color initColor)
        {
            brushPixels = new Color32[brushWidth * brushHeight];
            halfBrushWidth = brushWidth / 2;
            halfBrushHeight = brushHeight / 2;

            LoadBaseTexture();

            SetBrushColor(initColor);
        }

        /// <summary>
        /// For dynamically setting source texture,
        /// such as when we're testing, so jumping 
        /// into a specific scene with hair processed
        /// </summary>
        /// <param name="repl"></param>
        public void SetSourceTexture(Texture2D tex)
        {
            sourceTexture = tex;
            targetTexture = BuildTexBuffer(sourceTexture);
            targetMaterial.mainTexture = targetTexture;
        }

        /// <summary>
        /// For overlaying a texture instead of a colour.
        /// Must be read/write in import settings.
        /// </summary>
        /// <param name="repl"></param>
        public void SetReplacementTexture(Texture2D repl)
        {
            sourceReplacementTexture = repl;
            replacementTexture = BuildTexBuffer(sourceReplacementTexture);
        }

        public void LoadBaseTexture()
        {
            if (sourceTexture)
            {
                targetTexture = BuildTexBuffer(sourceTexture);
                targetMaterial.mainTexture = targetTexture;
            }

            if (sourceReplacementTexture)
                replacementTexture = BuildTexBuffer(sourceReplacementTexture);
        }

        public void Paint(float uvx, float uvy)
        {
            GetArea(uvx, uvy, out int xPos, out int yPos, out int blockWidth, out int blockHeight);
            ApplyBrush(xPos, yPos, blockWidth, blockHeight);
        }

        public void PaintFromSource(float uvx, float uvy)
        {
            CopyReplacementAreaToBrush(uvx, uvy, out int xPos, out int yPos, out int blockWidth, out int blockHeight);
            ApplyBrush(xPos, yPos, blockWidth, blockHeight);
        }


        public void BlendColour(float uvx, float uvy, Color32 col)
        {
            CopyAreaToBrush(uvx, uvy, out int xPos, out int yPos, out int blockWidth, out int blockHeight);

            for (int i = 0; i < brushPixels.Length; i++)
                brushPixels[i] = Utils.BlendColor(brushPixels[i], col);

            ApplyBrush(xPos, yPos, blockWidth, blockHeight);
        }

        public void Shade(float uvx, float uvy, float unitVal)
        {
            CopyAreaToBrush(uvx, uvy, out int xPos, out int yPos, out int blockWidth, out int blockHeight);

            for (int i = 0; i < brushPixels.Length; i++)
                brushPixels[i] = Utils.ShadeColor(brushPixels[i], unitVal);

            ApplyBrush(xPos, yPos, blockWidth, blockHeight);
        }

        public void Tint(float uvx, float uvy, float unitVal)
        {
            CopyAreaToBrush(uvx, uvy, out int xPos, out int yPos, out int blockWidth, out int blockHeight);

            for (int i = 0; i < brushPixels.Length; i++)
                brushPixels[i] = Utils.TintColor(brushPixels[i], unitVal);

            ApplyBrush(xPos, yPos, blockWidth, blockHeight);
        }

        public void SetBrushColor(Color32 col)
        {
            for (int i = 0; i < brushPixels.Length; i++)
                brushPixels[i] = col;

            if (col.a > 0 && !paintRecorder.ContainsKey(col))
                paintRecorder[col] = 0;
        }

        public Color32[] Pixels()
        {
            return targetTexture.GetPixels32();
        }

        /// <summary>
        /// Synchronous version. No good for mobile VR as
        /// it causes a brief freeze
        /// </summary>
        /// <returns></returns>
        public Dictionary<Color32, int> EvaluatePainting()
        {
            Color32[] pixels = targetTexture.GetPixels32();
            for (int i = 0; i < pixels.Length; i++)
            {
                Color32 col = pixels[i];
                if (paintRecorder.ContainsKey(col))
                    paintRecorder[col]++;
            }
            return paintRecorder;
        }

        /// <summary>
        /// This version is async-compatible as it doesn't
        /// make calls that can only be done on main thread
        /// (such as GetPixels)
        /// </summary>
        /// <param name="pixels"></param>
        /// <returns></returns>
        public Dictionary<Color32, int> EvaluatePainting(Color32[] pixels)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                Color32 col = pixels[i];
                if (paintRecorder.ContainsKey(col))
                    paintRecorder[col]++;
            }
            return paintRecorder;
        }

        /// <summary>
        /// Call this from an async function
        /// </summary>
        /// <returns></returns>
        public bool GetBlocks(out Color32[] blockA, out Color32[] blockB)
        {
            blockA = targetTexture.GetPixels32();
            blockB = replacementTexture.GetPixels32();

            return (blockA.Length == blockB.Length && blockA.Length > 0);
        }

        /// <summary>
        /// Here we check the difference between colours
        /// of the two blocks and thus judge how successful
        /// the paint job was
        /// </summary>
        /// <param name="blockA"></param>
        /// <param name="blockB"></param>
        /// <returns></returns>
        public float GetModified(Color32[] blockA, Color32[] blockB)
        {
            bool ok = (blockA.Length == blockB.Length && blockA.Length > 0);
            // Assume they're the same size
            Debug.Assert(ok, "Blocks not the same size in GetModified or block size is zero!");
            if (!ok)
            {
                Debug.LogFormat("blockA.Length: {0}, blockB.Length: {1}", blockA.Length, blockB.Length);
            }

            float count = 0;
            for (int i = 0; i < blockA.Length; i++)
            {
                if ((Color)blockA[i] == (Color)blockB[i])
                    count++;
            }
            return count / (float)blockA.Length;
        }

        /// <summary>
        /// Copy a region from writeable texture into brush
        /// for modification and subsequent stamping
        /// </summary>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="blockWidth"></param>
        /// <param name="blockHeight"></param>
        private void CopyAreaToBrush(float uvx, float uvy, 
            out int startX, out int startY, out int blockWidth, out int blockHeight)
        {
            GetArea(uvx, uvy, out startX, out startY, out blockWidth, out blockHeight);            

            // read into brushPixels
            Unity.Collections.NativeArray<Color32> pixels = targetTexture.GetRawTextureData<Color32>();
            int i = 0;
            for (int y = 0; y < blockHeight; y++)
            {
                for (int x = 0; x < blockWidth; x++)
                    brushPixels[i++] = pixels[(startY + y) * sourceTexture.width + startX + x];
            }
        }

        private void CopyReplacementAreaToBrush(float uvx, float uvy,
            out int startX, out int startY, out int blockWidth, out int blockHeight)
        {
            GetArea(uvx, uvy, out startX, out startY, out blockWidth, out blockHeight);

            // read into brushPixels
            Unity.Collections.NativeArray<Color32> pixels = replacementTexture.GetRawTextureData<Color32>();
            int i = 0;
            for (int y = 0; y < blockHeight; y++)
            {
                for (int x = 0; x < blockWidth; x++)
                    brushPixels[i++] = pixels[(startY + y) * sourceTexture.width + startX + x];
            }
        }

        /// <summary>
        /// Get a pixel's area def based on uv coords
        /// and brush offsets
        /// </summary>
        /// <param name="uvx"></param>
        /// <param name="uvy"></param>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="blockWidth"></param>
        /// <param name="blockHeight"></param>
        private void GetArea(float uvx, float uvy,
            out int startX, out int startY, out int blockWidth, out int blockHeight)
        {
            // Some issues here: the custom raycaster is returning uv
            // coords > 1.0, so they're now clamped
            int xPos = (int)((sourceTexture.width - 1) * Mathf.Clamp01(uvx));
            int yPos = (int)((sourceTexture.height - 1) * Mathf.Clamp01(uvy));

            startX = Mathf.Max(xPos - halfBrushWidth, 0);
            int endX = Mathf.Min(xPos + halfBrushWidth, sourceTexture.width - 1);
            startY = Mathf.Max(yPos - halfBrushHeight, 0);
            int endY = Mathf.Min(yPos + halfBrushHeight, sourceTexture.height - 1);
            blockWidth = endX - startX;
            blockHeight = endY - startY;
        }

        /// <summary>
        /// Copy contents of brush into writeable
        /// texture memory
        /// </summary>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="blockWidth"></param>
        /// <param name="blockHeight"></param>
        private void ApplyBrush(int startX, int startY, int blockWidth, int blockHeight)
        {
            targetTexture.SetPixels32(startX, startY, blockWidth, blockHeight, brushPixels);
            UpdateIfReady();
        }

        /// <summary>
        /// Periodically update the texture to give
        /// the app time to breath
        /// </summary>
        private void UpdateIfReady()
        {
            pixelApplyCounter += Time.deltaTime;
            if (pixelApplyCounter >= updateInterval)
            {
                targetTexture.Apply();
                pixelApplyCounter = 0f;
            }
        }

        private Texture2D BuildTexBuffer(Texture2D source)
        {
            Color32[] pixels = source.GetPixels32();
            Texture2D buffer = new Texture2D(source.width, source.height);
            buffer.SetPixels32(pixels);
            buffer.Apply();
            return buffer;
        }
    }
}