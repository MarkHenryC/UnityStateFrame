using UnityEngine;
using System.IO;

namespace QS
{
    [CreateAssetMenu()]
    public class DialogManager : ScriptableObject
    {
        public DialogOptions[] allOptions;
        public TextAsset inputScript;

        public void Parse()
        {
            string report = "";

            foreach (DialogOptions dlgOpt in allOptions)
            {
                if (!dlgOpt.name.StartsWith("_"))
                {
                    Debug.LogFormat("ID: {0}, comment text: {1}", dlgOpt.name, dlgOpt.entryAction.text);
                    
                    string data = string.Format(
                        "ID: {0}\nText: {1}\nAudio: {2}\nAnimation: {3}\n",
                        dlgOpt.name, dlgOpt.entryAction.text, dlgOpt.entryAction.voice.name, dlgOpt.entryAction.action);

                    if (data != null)
                    {
                        if (dlgOpt.options == null || dlgOpt.options.Length == 0)
                            data += "<END>\n\n";
                        else
                        {
                            data += "Options:\n\n";
                            foreach (DialogOptions.SelectionOptions option in dlgOpt.options)
                            {
                                Debug.LogFormat("    Option text: {0}", option.text);
                                string dest = (option.destination == null ? "<END>" : option.destination.name);

                                data += string.Format("\tOption: {0}\n\tPoints: {1}\n\tDestination ID: {2}\n\n",
                                    option.text, option.points, dest);
                            }
                        }
                        report += data;
                    }
                }
            }

            string ourFile = Application.dataPath + "/../" + this.name + ".txt";
            Debug.Log("Writing to: " + ourFile);
            File.WriteAllText(ourFile, report);
        }
    }
}