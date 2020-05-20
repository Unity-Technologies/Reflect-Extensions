using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Reflection;
using UnityEditor.SceneManagement;

namespace UnityEditor.Reflect.Extensions
{
    [CustomEditor(typeof(ReadMe))]
    [InitializeOnLoad]
    class ReadMeEditor : Editor
    {

        static string kShowedReadmeSessionStateName = "ReadmeEditor.showedReadMe";
        static float kSpace = 16f;

        static ReadMeEditor()
        {
            EditorApplication.delayCall += SelectReadmeAutomatically;
            EditorSceneManager.sceneOpened += ActiveSceneChange;
        }

        static void SelectReadmeAutomatically()
        {
            if (!SessionState.GetBool(kShowedReadmeSessionStateName, false))
            {
                var readme = SelectReadme();
                SessionState.SetBool(kShowedReadmeSessionStateName, true);

                if (readme && !readme.loadedLayout)
                {
                    //LoadLayout();
                    readme.loadedLayout = true;
                }
            }
        }

        static void ActiveSceneChange(Scene arg0, OpenSceneMode mode)
        {
            SelectReadme();
        }

        static void LoadLayout()
        {
            var assembly = typeof(EditorApplication).Assembly;
            var windowLayoutType = assembly.GetType("UnityEditor.WindowLayout", true);
            var method = windowLayoutType.GetMethod("LoadWindowLayout", BindingFlags.Public | BindingFlags.Static);
            method.Invoke(null, new object[] { Path.Combine(Application.dataPath, "InstructionalInfo/InfoLayout.wlt"), false });
        }

        [MenuItem("Reflect/Sequencing Setup Instructions %h")]
        static ReadMe SelectReadme()
        {
            var _scene = EditorSceneManager.GetActiveScene();
            var sceneNameParts = _scene.name.Split(' ');

            foreach (var s in sceneNameParts)
            {
                string searchReadMe = s + " t:ReadMe";
                var ids = AssetDatabase.FindAssets(searchReadMe);
                if (ids.Length > 0)
                {
                    var readmeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(ids[0]));
                    Selection.objects = new UnityEngine.Object[] { readmeObject };
                    return (ReadMe)readmeObject;
                }
            }

            return null;
        }

        protected override void OnHeaderGUI()
        {
            var readme = (ReadMe)target;
            Init();

            var iconWidth = Mathf.Min(EditorGUIUtility.currentViewWidth / 3f - 20f, 128f);

            GUILayout.BeginHorizontal("In BigTitle");
            {
                GUILayout.Label(readme.icon, GUILayout.Width(iconWidth), GUILayout.Height(iconWidth));
                GUILayout.Label(readme.title, TitleStyle);
            }
            GUILayout.EndHorizontal();
        }

        public override void OnInspectorGUI()
        {
            var readme = (ReadMe)target;
            Init();

            foreach (var section in readme.sections)
            {
                if (!string.IsNullOrEmpty(section.heading))
                {
                    GUILayout.Label(section.heading, HeadingStyle);
                }
                if (!string.IsNullOrEmpty(section.text))
                {
                    GUILayout.Label(section.text, BodyStyle);
                }
                if (!string.IsNullOrEmpty(section.linkText))
                {
                    if (LinkLabel(new GUIContent(section.linkText)))
                    {
                        if (!string.IsNullOrEmpty(section.url))
                        {
                            if (section.url.StartsWith("http"))
                                Application.OpenURL(section.url);
                            else if (section.url.StartsWith("ExecuteMenuItem:"))
                            {
                                string menuItem = section.url.Replace("ExecuteMenuItem:", "");
                                EditorApplication.ExecuteMenuItem(menuItem);
                            }
                            else
                                GetProjectObjects(section.url);
                        }
                        else
                            GetSceneRootGameObjects(section.linkText);
                    }
                }
                GUILayout.Space(kSpace);
            }
        }

        bool m_Initialized;

        GUIStyle LinkStyle { get { return m_LinkStyle; } }
        [SerializeField] GUIStyle m_LinkStyle;

        GUIStyle TitleStyle { get { return m_TitleStyle; } }
        [SerializeField] GUIStyle m_TitleStyle;

        GUIStyle HeadingStyle { get { return m_HeadingStyle; } }
        [SerializeField] GUIStyle m_HeadingStyle;

        GUIStyle BodyStyle { get { return m_BodyStyle; } }
        [SerializeField] GUIStyle m_BodyStyle;

        void Init()
        {
            if (m_Initialized)
                return;

            m_BodyStyle = new GUIStyle(EditorStyles.label);
            m_BodyStyle.wordWrap = true;
            m_BodyStyle.richText = true;
            m_BodyStyle.fontSize = 14;

            m_TitleStyle = new GUIStyle(m_BodyStyle);
            m_TitleStyle.fontSize = 26;

            m_HeadingStyle = new GUIStyle(m_BodyStyle);
            m_HeadingStyle.fontSize = 18;

            m_LinkStyle = new GUIStyle(m_BodyStyle);
            m_LinkStyle.wordWrap = false;
            // Match selection color which works nicely for both light and dark skins
            m_LinkStyle.normal.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
            m_LinkStyle.stretchWidth = false;

            m_Initialized = true;
        }

        bool LinkLabel(GUIContent label, params GUILayoutOption[] options)
        {
            var position = GUILayoutUtility.GetRect(label, LinkStyle, options);

            Handles.BeginGUI();
            Handles.color = LinkStyle.normal.textColor;
            Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
            Handles.color = Color.white;
            Handles.EndGUI();

            EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);

            return GUI.Button(position, label, LinkStyle);
        }

        void GetProjectObjects(string assetName)
        {
            var ids = AssetDatabase.FindAssets(assetName);
            if (ids.Length > 0)
            {
                var readmeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(ids[0]));
                Selection.objects = new UnityEngine.Object[] { readmeObject };
                EditorGUIUtility.PingObject(readmeObject);
            }
        }

        List<Object> matchingObjects;
        void GetSceneRootGameObjects(string objectName)
        {
            List<GameObject> rootObjects = new List<GameObject>();
            matchingObjects = new List<Object>();
            Scene scene = EditorSceneManager.GetActiveScene();
            scene.GetRootGameObjects(rootObjects);
            foreach (var go in rootObjects)
            {
                if (objectName == go.name)
                    matchingObjects.Add(go);
                SearchAllObjects(go.transform, objectName);
            }

            if (matchingObjects.Any())
            {
                Selection.objects = matchingObjects.ToArray();
                foreach (var o in Selection.objects)
                {
                    EditorGUIUtility.PingObject(o);
                }
            }
        }

        void SearchAllObjects(Transform go, string matchName)
        {
            foreach (Transform trans in go.transform)
            {
                if (matchName == trans.name)
                {
                    matchingObjects.Add(trans.gameObject);
                }
                SearchAllObjects(trans, matchName);
            }
        }
    }
}