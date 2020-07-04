using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
	public class GrabbableManager : MonoBehaviour 
	{
        public Graspable[] grabbables;
        public bool lockDestinationPositions, lockHomePositions;

        public void Each(Action<Graspable> action)
        {
            Utils.Each<Graspable>(transform, action);
        }

        public int GetGrabbableChildCount()
        {
            return Utils.GetComponentCount<Graspable>(transform);
        }

        public void LoadFromSubfolder()
        {
            grabbables = Utils.GetArrayOf<Graspable>(transform);
        }

        public void MoveAllToHome()
        {
            foreach (var g in grabbables)
                g.TransformToHome();
        }

        public void MoveAllToDest()
        {
            foreach (var g in grabbables)
                g.TransformToDest();
        }

        public void SetAllAsHome()
        {
            if (!lockHomePositions)
            {
                foreach (var g in grabbables)
                    g.SetHomeTransform();
            }
        }

        public void SetAllAsDest()
        {
            if (!lockDestinationPositions)
            {
                foreach (var g in grabbables)
                    g.SetDestTransform();
            }
        }
    }
}