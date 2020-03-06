using UnityEngine;

namespace UnityEditor.Reflect.Extensions
{
    public class ReflectSettingsWizardWindow : EditorWindow
    {
        readonly string fixButtonLabel = "FIX";

        [MenuItem("Reflect/Tools/Settings Wizard")]
        public static void OpenWindow()
        {
            var window = GetWindow<ReflectSettingsWizardWindow>();
            window.Show();
            window.titleContent = new GUIContent("Reflect Settings Wizard");
        }

        public void OnGUI()
        {
            var target = EditorUserBuildSettings.activeBuildTarget;
            var group = BuildPipeline.GetBuildTargetGroup(target);

            // Api Compatibility Level
            if (PlayerSettings.GetApiCompatibilityLevel(group) != ApiCompatibilityLevel.NET_4_6)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox("Api Compatibility Level should be set to .NET 4.x in Player Settings", MessageType.Warning);
                if (GUILayout.Button(fixButtonLabel))
                    PlayerSettings.SetApiCompatibilityLevel(group, ApiCompatibilityLevel.NET_4_6);
                GUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("Api Compatibility Level is set to .NET 4.x in Player Settings", MessageType.Info);
            }

            // TODO : ARM64 Target

            // TODO : iOS Minimum Version

            // TODO : Android  Minimum Version

            // TODO : add warnings/fixes for other Reflect related settings
        }
    }
}