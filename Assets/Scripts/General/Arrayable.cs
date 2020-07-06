using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
	public class Arrayable : MonoBehaviour 
	{
        public int xCount, yCount, zCount;
        public float xOffset, yOffset, zOffset;
        public Vector3 startPos;
        public GameObject template;
        public bool brickStyle; // Hacky shortcut for brickwall-style stacking
        public AudioClip soundEffect;

        private AudioSource audioSource;

        private void Awake()
        {
            template.SetActive(false);
            audioSource = GetComponent<AudioSource>();
            if (audioSource && soundEffect)
                audioSource.clip = soundEffect;
        }

        public void SetStartPosition()
        {
            startPos = template.transform.position;
        }

        /// <summary>
        /// This version is for Editor testing
        /// </summary>
        public void Generate()
        {
            if (yCount == 0)
                yCount = 1;
            if (zCount == 0)
                zCount = 1;

            float xPos = 0f, yPos = 0f, zPos = 0f;
            float xIndent = brickStyle ? (xOffset / 2f) : 0f;

            for (int z = 0; z < zCount; z++)
            {
                yPos = 0f;
                for (int y = 0; y < yCount; y++)
                {
                    xPos = 0f;
                    for (int x = 0; x < xCount; x++)
                    {
                        GameObject go = Instantiate(template, transform);
                        go.SetActive(true);
                        float indentedX = (y % 2 != 0) ? xIndent : 0f;
                        go.transform.position = new Vector3(startPos.x + xPos + indentedX, startPos.y + yPos, startPos.z + zPos);
                        xPos += xOffset;

                        if (audioSource && soundEffect)
                            audioSource.Play();
                    }
                    yPos += yOffset;
                }
                zPos += zOffset;
            }
        }

        public void Generate(float delay)
        {
            StartCoroutine(CrGenerate(delay));
        }

        public void Clear(bool immediate = false)
        {
            var gol = new List<GameObject>();
            for (int i = 0; i < transform.childCount; i++)
                gol.Add(transform.GetChild(i).gameObject);

            foreach (var v in gol)
            {
                if (immediate)
                    DestroyImmediate(v);
                else
                    Destroy(v);
            }
        }

        private IEnumerator CrGenerate(float delay)
        {
            if (yCount == 0)
                yCount = 1;
            if (zCount == 0)
                zCount = 1;

            float xPos = 0f, yPos = 0f, zPos = 0f;
            float xIndent = brickStyle ? (xOffset / 2f) : 0f;

            var crDelay = new WaitForSeconds(delay);

            for (int z = 0; z < zCount; z++)
            {
                yPos = 0f;
                for (int y = 0; y < yCount; y++)
                {
                    xPos = 0f;
                    for (int x = 0; x < xCount; x++)
                    {
                        GameObject go = Instantiate(template, transform);
                        go.SetActive(true);
                        float indentedX = (y % 2 != 0) ? xIndent : 0f;
                        go.transform.position = new Vector3(startPos.x + xPos + indentedX, startPos.y + yPos, startPos.z + zPos);
                        xPos += xOffset;

                        if (audioSource && soundEffect)
                            audioSource.Play();

                        yield return crDelay;
                    }
                    yPos += yOffset;
                }
                zPos += zOffset;
            }
        }

        private void OnDestroy()
        {
            Clear();
        }
    }
}