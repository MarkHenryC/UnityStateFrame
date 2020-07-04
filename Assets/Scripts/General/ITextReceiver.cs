using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QS
{
    /// <summary>
    /// Usually this is implemented in a State
    /// script, so objects can share text
    /// prompts. If string is null, text panel
    /// should be hidden, and vice-versa when non-null
    /// </summary>
	public interface ITextReceiver
	{
        void ReceiveRolloverText(string text);
        void ReceiveClickdownText(string text);
    }
}