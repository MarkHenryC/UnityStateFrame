using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
	public class Mixer : Receivable 
	{
        public DynamicMaterial ourMaterial;
        public Filler mixLevel;

        public Color ColorMix
        {
            get { return colorMix.NormalizedOpaque(); }
            private set { colorMix = value; }
        }
        public float CurrentFill { get; private set; }

        private Color colorMix;

        public void AddColor(Color col, float fill = 0f)
        {
            colorMix += (col * fill);

            CurrentFill += fill;

            if (ourMaterial)
                ourMaterial.SetColour(ColorMix);
            if (mixLevel)
                mixLevel.Fill(CurrentFill);
        }

    }
}