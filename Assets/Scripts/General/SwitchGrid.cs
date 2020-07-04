using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
	public class SwitchGrid : MonoBehaviour 
	{
        public GameObject template;
        public int xRows, yRows, zRows;
        public float xSpacing, ySpacing, zSpacing;
        public bool numbered = true;
        public bool zeroBased = false;
        public string[] legends;

        public Action<SwitchGridItem> OnChildSelection;

        public void CreateGrid()
        {
            int number = zeroBased ? -1 : 0;
            Vector3 localPosition = transform.InverseTransformPoint(template.transform.position);
            ClearGrid();
            int units = xRows * yRows * zRows;
            bool useLegends = false;
            int legendIndex = 0;
            if (legends != null && legends.Length >= units)
                useLegends = true;

            if (zRows == 0)
                zRows = 1;

            if (yRows == 0)
                yRows = 1;

            for (int z = 0; z < zRows; z++)
            {
                for (int y = 0; y < yRows; y++)
                {
                    for (int x = 0; x < xRows; x++)
                    {
                        GameObject instance = Instantiate<GameObject>(template, transform);
                        instance.transform.localPosition = localPosition + new Vector3(-x * xSpacing, y * ySpacing, z * zSpacing);
                        if (useLegends)
                        {
                            instance.name = legends[legendIndex++];
                        }
                        else if (numbered)
                        {
                            number++;
                            instance.name = number.ToString();
                        }
                        TextBox textBox = instance.GetComponent<TextBox>();
                        if (textBox)
                            textBox.SetText(instance.name);
                    }
                }
            }

            template.SetActive(false);
        }

        public void ClearGrid()
        {
            template.SetActive(false); // In case it's wihin this transform, don't delete (see activeSelf below)
            List<GameObject> gridObjects = new List<GameObject>();
            int childCount = transform.childCount;
            for (int i = 0; i < transform.childCount; i++)
            {
                var childObject = transform.GetChild(i).gameObject;
                if (childObject.activeSelf)
                    gridObjects.Add(transform.GetChild(i).gameObject);
            }
            foreach (var g in gridObjects)
                DestroyImmediate(g);
            template.SetActive(true);
        }
	}
}