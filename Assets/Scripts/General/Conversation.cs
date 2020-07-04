using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace QS
{
    [Serializable]
    public class Option
    {
        public string Text;
        public int Points;
        public string DestID;
    }

    [Serializable]
    public class ConversationNode
    {
        public string ID;
        public string Text;
        public string Audio;
        public string Animation;
        public Option[] Options;
    }
        
    [Serializable]
    public class Conversation
	{
        public ConversationNode[] Nodes;

        /// <summary>
        /// Runtime loader
        /// </summary>
        /// <param name="jsonPath"></param>
        /// <returns></returns>
        public static Conversation Load(string jsonPath)
        {
            TextAsset textAsset = Resources.Load<TextAsset>(jsonPath);

            string data = textAsset.text;
            if (data.Usable())
                return JsonUtility.FromJson<Conversation>(data);
            else
                return null;
        }

        /// <summary>
        /// Editor loader
        /// </summary>
        /// <param name="textAsset"></param>
        /// <returns></returns>
        public static Conversation Load(TextAsset textAsset)
        {
            string data = textAsset.text;
            if (data.Usable())
                return JsonUtility.FromJson<Conversation>(data);
            else
                return null;
        }

        public ConversationNode GetNode(string name)
        {
            if (name.Useless())
                return null; // So we don't print a warning for not found, as null is a legit terminator

            foreach (var n in Nodes)
            {
                if (n.ID == name)
                    return n;
            }

            Debug.LogWarningFormat("Node {0} not found", name);

            return null;
        }

        public ConversationNode GetFirstNode()
        {
            if (Nodes.Length > 0)
                return Nodes[0];
            else
                return null;
        }
	}
}