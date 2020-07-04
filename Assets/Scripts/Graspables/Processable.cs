using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QS
{
    public class Processable : Placeable
    {        
        public ProcessingType processingType;

        public enum ProcessingType { None, Sliced, Chopped, Crushed };
    }
}