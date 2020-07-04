using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Build scripts. Accessed via Tools menu
/// </summary>
namespace QS
{
    public class BuildCustom
    {
        public static string[] levels = new string[] 
        {
            "Assets/Scenes/Start.unity"
        };

        private static readonly string projectPath = Application.dataPath + "/../";
        private static string versionTextPath = projectPath + @"/latest_build_number.txt";
        private static readonly string productName = "YourProduct";
        private static string resolvedProjectPath;
        private static readonly string apkFile = productName + ".apk";
        private static int buildID;

        private const string APP_ID = "com.yourcompany.yourapp";
        private const string DFLT_OBB = "YourProduct.main.obb";

        /// <summary>
        /// Oculus store build, no transfer
        /// </summary>
        [MenuItem("Tools/GVR|Go Store Build - no transfer")]
        public static void BuildGearVrHidden()
        {
            resolvedProjectPath = ResolveOutputPath("./", apkFile);

            SetBuildName();
            PrepareAndroid();

            BuildPipeline.BuildPlayer(levels, resolvedProjectPath,
                BuildTarget.Android, BuildOptions.ShowBuiltPlayer);

            SetObbName();
        }

        /// <summary>
        /// Store build. Upload to dev testing channel
        /// </summary>
        [MenuItem("Tools/Go Store Build - upload to Testing")]
        public static void BuildGearVrHiddenUpload()
        {
            BuildGearVrHidden();
            RunBatchCopy("upload-Testing");
        }

        /// <summary>
        /// Store build. Transfer with obb to device
        /// </summary>
        [MenuItem("Tools/Go Store Build - usb transfer")]
        public static void BuildGearVrHiddenUsb()
        {
            BuildGearVrHidden();
            RunBatchCopy("usb");
        }

        [MenuItem("Tools/NO BUILD: upload to Testing")]
        public static void Upload()
        {
            RunBatchCopy("upload-Testing");
        }

        [MenuItem("Tools/NO BUILD: upload via USB")]
        public static void UploadUsb()
        {
            RunBatchCopy("usb");
        }

#if NO_NEW_BUILD

        [MenuItem("Tools/GVR|usb transfer only")]
        public static void TransferUsb()
        {
            RunBatchCopy("usb");
        }

        [MenuItem("Tools/GVR|Go rename obb file")]
        public static void RenameObb()
        {
            SetObbName();
        }
#endif
        private static void PrepareAndroid(bool sign = true)
        {           
            if (sign)
            {
                PlayerSettings.Android.keystorePass = "your_keystore_pass";
                PlayerSettings.Android.keyaliasName = "your_alias_name";
                PlayerSettings.Android.keyaliasPass = "your_alisa_pass";
            }
            else
            {
                PlayerSettings.Android.keystorePass = "";
                PlayerSettings.Android.keyaliasName = "";
                PlayerSettings.Android.keyaliasPass = "";
            }

            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, APP_ID);
        }

        private static string ResolveOutputPath(string subPath, string binaryName)
        {
            return System.IO.Path.GetFullPath(projectPath + subPath + binaryName);
        }

        private static void RunBatchCopy(string type)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "install_" + type + ".bat";
            process.StartInfo.WorkingDirectory = projectPath;
            process.StartInfo.Arguments = GetBuildId().ToString();
            process.Start();
        }

        private static void SetBuildName()
        {            
            buildID = GetBuildId();
            buildID++;

            File.WriteAllText(versionTextPath, buildID.ToString());

            float version = buildID / 100f;
            string bundleVersion = version.ToString();

            PlayerSettings.productName = productName;
            PlayerSettings.bundleVersion = bundleVersion;
            PlayerSettings.Android.bundleVersionCode = buildID;

            Debug.LogFormat("App version: {0}, bundle ID: {1}", version, bundleVersion);
        }

        private static void SetObbName()
        {
            if (File.Exists(DFLT_OBB))
                File.Move(DFLT_OBB, "main." + GetBuildId().ToString() + "." + APP_ID + ".obb");
        }

        private static int GetBuildId()
        {
            string[] lines = File.ReadAllLines(versionTextPath);
            Debug.Assert(lines.Length > 0, "Where is data in latest_build_number.txt?");

            if (!int.TryParse(lines[0], out buildID))
                buildID = PlayerSettings.Android.bundleVersionCode; // Not desirable as ver is cached with branch

            return buildID;
        }
    }
}