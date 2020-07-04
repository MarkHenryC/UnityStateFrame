using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QS
{
	public class ActivityNight : ActivityBase
	{
        public bool testLightOn;

        public override void Initialize()
        {
            base.Initialize();

            if (!testLightOn)
                ActivitySettings.Asset.PushAmbientLight(new Color(.01f, .01f, .01f));
        }

        public override void Finish()
        {
            base.Finish();

            if (!testLightOn)
                ActivitySettings.Asset.PopAmbientLight();
        }
    }
}