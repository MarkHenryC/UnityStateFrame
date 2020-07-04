using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
	public class PrismMapManager : MonoBehaviour 
	{
        public Renderer templateRenderer;
        public int mapDimensionsX = 32;
        public int mapDimensionsY = 2;
        public Material templateMaterial;
        public Texture2D templateTexture;
        public float spacing, zSetback;
        public Transform centreLocation;
        public SphereCollider head, neck;
        public int rowWidth = 16;
        public Texture2D writeableTexture;
        public float lowerYAtEdge = .179f; // linear but should be curved

        public void SetTextureMapping()
        {
            float incrementX = 1f / mapDimensionsX;
            float incrementY = 1f / mapDimensionsY;

            float startPosX = -(rowWidth * spacing) / 2f;
            float startPosY = -(mapDimensionsY * spacing) / 2f;
            float zMove = zSetback;

            writeableTexture = new Texture2D(templateTexture.width, templateTexture.height);
            Color32[] pixels = templateTexture.GetPixels32();
            writeableTexture.SetPixels32(pixels);
            writeableTexture.Apply();

            templateRenderer.gameObject.SetActive(true);

            for (int y = 0; y < mapDimensionsY; y++)
            {
                int colCount = 0;
                float xOffset = y * (spacing / 2f);

                for (int x = 0; x < mapDimensionsX; x++)
                {                    
                    if (colCount == rowWidth)
                    {
                        colCount = 0;
                        zMove += spacing;
                    }
                    Renderer r = Instantiate<Renderer>(templateRenderer);
                    r.sharedMaterial = new Material(templateMaterial)
                    {
                        mainTextureOffset = new Vector2((float)x * incrementX, (float)y * incrementY),
                        mainTexture = writeableTexture
                    };
                    r.gameObject.name = string.Format("{0}_{1}", x, y);
                    r.transform.SetParent(this.transform, false);
                    r.transform.localRotation = Quaternion.identity;

                    float midX = (float)rowWidth / 2f;
                    float deviationX = Mathf.Abs((float)colCount - midX);
                    float unitDeviation = deviationX / (float)midX;

                    float ySubtractY = unitDeviation * lowerYAtEdge;
                    
                    r.transform.localPosition = centreLocation.localPosition + new Vector3(xOffset + startPosX + colCount * spacing, -ySubtractY, zSetback + startPosY + y * spacing);
                    var scale = r.transform.localScale;
                    r.transform.localScale = new Vector3(scale.x, scale.y, scale.z * (1f + (1f - unitDeviation)));
                    var cloth = r.gameObject.GetComponent<Cloth>();
                    var colliders = new ClothSphereColliderPair[1];
                    colliders[0] = new ClothSphereColliderPair(head, neck);
                    cloth.sphereColliders = colliders;
                    colCount++;
                }
            }

            templateRenderer.gameObject.SetActive(false);
        }

        public void ClearAll()
        {
            if (transform.childCount > 0)
            {
                GameObject[] existing = new GameObject[transform.childCount];

                for (int n = 0; n < transform.childCount; n++)
                {
                    var g = transform.GetChild(n).gameObject;
                    if (g != centreLocation)
                        existing[n] = g;
                }
                for (int p = 0; p < existing.Length; p++)
                {
                    if (existing[p])
                        DestroyImmediate(existing[p]);
                }
                existing = null;
            }
        }
    }
}