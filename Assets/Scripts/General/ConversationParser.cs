using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace QS
{
    [CreateAssetMenu()]
    public class ConversationParser : ScriptableObject
    {
        public TextAsset conversationAsset;
        public string startingNodeName;
        public int maxPathChunks = 100;

        private Conversation conversation;
        private readonly string currentPath;
        private int pathCount;

        private List<string> pathList;

        public void Parse()
        {
            if (conversationAsset)
            {
                conversation = Conversation.Load(conversationAsset);
                if (conversation != null)
                {
                    string data = "";
                    pathList = new List<string>();

                    ConversationNode firstNode;
                    if (startingNodeName.Useless())
                        firstNode = conversation.GetFirstNode();
                    else
                        firstNode = conversation.GetNode(startingNodeName);

                    FollowPath(firstNode, "");

                    foreach (string p in pathList)
                        data += p;

                    string ourFile = Application.dataPath + "/../" + name + ".txt";
                    Debug.Log("Writing to: " + ourFile);
                    File.WriteAllText(ourFile, data);

                }
                else
                    Debug.LogError("No conversation created from asset");
            }
            else
                Debug.LogError("No conversation asset specified");
        }

        public void ParseBrief()
        {
            if (conversationAsset)
            {
                conversation = Conversation.Load(conversationAsset);
                if (conversation != null)
                {
                    string data = "";
                    pathList = new List<string>();

                    ConversationNode firstNode;
                    if (startingNodeName.Useless())
                        firstNode = conversation.GetFirstNode();
                    else
                        firstNode = conversation.GetNode(startingNodeName);

                    FollowPathBrief(firstNode, "");

                    foreach (string p in pathList)
                        data += p;
                    string ourFile = Application.dataPath + "/../" + name + "_brief.txt";
                    Debug.Log("Writing to: " + ourFile);
                    File.WriteAllText(ourFile, data);

                }
                else
                    Debug.LogError("No conversation created from asset");
            }
            else
                Debug.LogError("No conversation asset specified");
        }

        public void CountPaths()
        {
            pathCount = 0;
            CountPaths(conversation.GetNode(startingNodeName));
            Debug.Log("Number of complete paths: " + pathCount);
        }

        /// <summary>
        /// Shows entire Q & A path
        /// </summary>
        /// <param name="node"></param>
        /// <param name="pathText"></param>
        private void FollowPath(ConversationNode node, string pathText)
        {
            int sections = CountChar(pathText, '>');
            if (sections > maxPathChunks)
            {
                pathText += "<...>\r\n";
                return;
            }

            for (int i = 0; i < node.Options.Length; i++)
            {
                string nodeOption = pathText + node.Text + " > " + node.Options[i].Text + " > ";

                if (node.Options[i].DestID.Useless())
                {
                    pathList.Add(nodeOption + "<END>\r\n");
                }
                else
                {
                    ConversationNode nextNode = conversation.GetNode(node.Options[i].DestID);
                    if (nextNode == null)
                        Debug.Log("Null find for node option " + node.Options[i].Text);
                    else
                        FollowPath(nextNode, nodeOption);
                }
            }
        }

        /// <summary>
        /// Shows abbreviated version; code for question,
        /// index for chosen answer option
        /// </summary>
        /// <param name="node"></param>
        /// <param name="pathText"></param>
        private void FollowPathBrief(ConversationNode node, string pathText)
        {
            int sections = CountChar(pathText, '>');
            if (sections > maxPathChunks)
            {
                pathText += "<...>\r\n";
                return;
            }

            for (int i = 0; i < node.Options.Length; i++)
            {
                string nodeOption = pathText + node.ID + " > " + i + " > ";
                
                if (node.Options[i].DestID.Useless())
                {
                    pathList.Add(nodeOption + "<END>\r\n");
                }
                else
                {
                    ConversationNode nextNode = conversation.GetNode(node.Options[i].DestID);
                    if (nextNode == null)
                        Debug.Log("Null find for node option " + node.Options[i].Text);
                    else
                        FollowPathBrief(nextNode, nodeOption);
                }
            }
        }

        private void CountPaths(ConversationNode node)
        {
            for (int i = 0; i < node.Options.Length; i++)
            {
                if (node.Options[i].DestID.Useless())
                {
                    Debug.Log("Terminated at " + node.Options[i].Text);
                    pathCount++;
                }
                else
                {
                    ConversationNode nextNode = conversation.GetNode(node.Options[i].DestID);
                    if (nextNode == null)
                        Debug.Log("Null find for node option " + node.Options[i].Text);
                    else
                        CountPaths(nextNode);
                }
            }
        }

        private int CountChar(string text, char c)
        {
            int count = 0;
            while (count < text.Length && text[count] == c)
                count++;
            return count;
        }
    }
}