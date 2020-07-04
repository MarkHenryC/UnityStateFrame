using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
    public interface IUnitValueProvider
    {
        void AddListener(Action<IUnitValueProvider, float> af);
        void RemoveListener(Action<IUnitValueProvider, float> af);
    }
}