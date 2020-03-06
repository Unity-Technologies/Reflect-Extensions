using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.XR.ARSubsystems;
using System.ComponentModel;
using UnityEngine.Reflect.Extensions;
using UnityEngine;

namespace UnityEditor.Reflect.Extensions
{
    /// <summary>
    /// Custom editor for the ImageTrackingManager to allow selection and pairing of the appropriate Image Tracking Handler and image target
    /// </summary>
    [CustomEditor(typeof(ImageTrackingManager))]
    class ImageTrackingManagerEditor : Editor
    {
        SerializedProperty referenceImageLibrary;
        XRReferenceImageLibrary library;
        SerializedProperty lookupInfo;
        List<ImageTrackingManager.ImageTargetHandlerInfo> lookupList = new List<ImageTrackingManager.ImageTargetHandlerInfo>();
        List<string> possibleHandlerNames = new List<string>();
        List<Type> possibleHandlerTypes = new List<Type>();
        int[] drawIndex;
        Dictionary<string, string> previousLibrary;

        void OnEnable()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                referenceImageLibrary = serializedObject.FindProperty("referenceImageLibrary");
                if (referenceImageLibrary.objectReferenceValue != null)
                {
                    library = referenceImageLibrary.objectReferenceValue as XRReferenceImageLibrary;
                    drawIndex = new int[library.count];
                }
                possibleHandlerNames = GetAllHandlerNames();
                possibleHandlerNames.Insert(0, "None");
                lookupInfo = serializedObject.FindProperty("handlerInfoList");
                if (library != null)
                    GetLookUpList();
            }
        }

        public override void OnInspectorGUI()
        {
            try
            {
                serializedObject.Update();
                base.OnInspectorGUI();

                if (referenceImageLibrary.objectReferenceValue != null && referenceImageLibrary.objectReferenceValue != library)
                {
                    SavePreviousLibrary();
                    OnEnable();
                }

                if (library != null && library.count > 0)
                {
                    GUILayout.Label("Assign Image Target Handler to each Image", EditorStyles.boldLabel);
                    for (int i = 0; i < library.count; i++)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Space(20);
                            var referenceImage = library[i];
                            var texturePath = AssetDatabase.GUIDToAssetPath(referenceImage.textureGuid.ToString("N"));
                            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                            if (texture != null)
                            {
                                GUILayout.Label(texture, GUILayout.Width(50), GUILayout.Height(50));
                                GUILayout.Space(10);
                            }

                            using (new EditorGUILayout.VerticalScope())
                            {
                                GUILayout.Label(library[i].name);
                                using (var changeCheck = new EditorGUI.ChangeCheckScope())
                                {
                                    EditorGUIUtility.labelWidth = 140;
                                    drawIndex[i] = EditorGUILayout.Popup("Image Target Handler:", drawIndex[i], possibleHandlerNames.ToArray(), GUILayout.Width(320));
                                    if (drawIndex[i] > 0)
                                    {
                                        var description = GetDescription(possibleHandlerTypes[drawIndex[i] - 1]);
                                        if (!string.IsNullOrEmpty(description))
                                            GUILayout.Label(description);
                                    }
                                    EditorGUIUtility.labelWidth = 0;
                                    if (changeCheck.changed)
                                    {
                                        SaveLookUpList(i);

                                        serializedObject.ApplyModifiedProperties();
                                        Undo.RecordObject(target, "Update target handler");
                                        EditorUtility.SetDirty(target);
                                    }
                                }
                            }
                        }
                        if (i < library.count - 1)
                            GUILayout.Space(15);
                    }
                }
                serializedObject.ApplyModifiedProperties();
            }
            catch
            {
                Repaint();
            }
        }

        // Look for existing lookupInfo sync up with the reference image library
        void GetLookUpList()
        {
            if (lookupInfo == null || lookupInfo.arraySize < 1)
            {
                for (int i = 0; i < library.count; i++)
                {
                    lookupInfo.InsertArrayElementAtIndex(i);
                    var lookUpProperty = lookupInfo.GetArrayElementAtIndex(i);
                    var imageProperty = lookUpProperty.FindPropertyRelative("imageGUID");
                    var handlerNameProperty = lookUpProperty.FindPropertyRelative("handlerName");
                    imageProperty.stringValue = library[i].guid.ToString();
                    // Set to None
                    handlerNameProperty.stringValue = possibleHandlerNames[0];
                }
            }
            else
            {
                if (library.count < 1)
                    lookupInfo.ClearArray();

                // Compare array sizes and contents of library and lookupInfo
                SyncUpArrays();

                for (int i = 0; i < lookupInfo.arraySize; i++)
                {
                    var lookUpProperty = lookupInfo.GetArrayElementAtIndex(i);
                    var imageProperty = lookUpProperty.FindPropertyRelative("imageGUID");
                    var handlerNameProperty = lookUpProperty.FindPropertyRelative("handlerName");

                    if (imageProperty.stringValue != library[i].guid.ToString())
                    {
                        imageProperty.stringValue = library[i].guid.ToString();
                        handlerNameProperty.stringValue = possibleHandlerNames[0];
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(handlerNameProperty.stringValue))
                        {
                            drawIndex[i] = 0;
                        }
                        else
                        {
                            int findIndex = possibleHandlerNames.IndexOf(handlerNameProperty.stringValue);
                            if (findIndex == -1)
                                drawIndex[i] = 0;
                            else
                                drawIndex[i] = findIndex;
                        }
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        // Sync up the reference image library and the Handler Info list
        void SyncUpArrays()
        {
            // Store temporary lookup of what is already in lookupInfo
            Dictionary<string, string> tempLookUpInfo = new Dictionary<string, string>();
            for (int i = 0; i < lookupInfo.arraySize; i++)
            {
                var lookUpProperty = lookupInfo.GetArrayElementAtIndex(i);
                var imageProperty = lookUpProperty.FindPropertyRelative("imageGUID");
                var handlerNameProperty = lookUpProperty.FindPropertyRelative("handlerName");
                if (!tempLookUpInfo.ContainsKey(imageProperty.stringValue))
                    tempLookUpInfo.Add(imageProperty.stringValue, handlerNameProperty.stringValue);
            }

            // If the reference image library is larger
            if (library.count > lookupInfo.arraySize)
            {
                int initialSize = lookupInfo.arraySize;
                for (int i = initialSize; i < library.count; i++)
                    lookupInfo.InsertArrayElementAtIndex(i);
            }

            // Look for previous images and handler names
            for (int i = 0; i < library.count; i++)
            {
                var lookUpProperty = lookupInfo.GetArrayElementAtIndex(i);
                var imageProperty = lookUpProperty.FindPropertyRelative("imageGUID");
                var handlerNameProperty = lookUpProperty.FindPropertyRelative("handlerName");

                // The reference image library has changed
                if (library[i].guid.ToString() != imageProperty.stringValue)
                {
                    if (previousLibrary != null && previousLibrary.ContainsKey(library[i].textureGuid.ToString()))
                    {
                        var guidToLookUp = previousLibrary[library[i].textureGuid.ToString()];

                        if (tempLookUpInfo.ContainsKey(guidToLookUp))
                            handlerNameProperty.stringValue = tempLookUpInfo[guidToLookUp];
                        else
                            handlerNameProperty.stringValue = "";
                    }
                    imageProperty.stringValue = library[i].guid.ToString();
                }
            }

            // If the lookupInfo is larger than new reference image library
            if (lookupInfo.arraySize > library.count)
            {
                int total = lookupInfo.arraySize;
                for (int i = library.count; i < total; i++)
                    lookupInfo.DeleteArrayElementAtIndex(i);
            }

            serializedObject.ApplyModifiedProperties();
        }

        // Entry changed so save the update
        void SaveLookUpList(int element)
        {
            var lookUpProperty = lookupInfo.GetArrayElementAtIndex(element);
            var imageProperty = lookUpProperty.FindPropertyRelative("imageGUID");
            var handlerNameProperty = lookUpProperty.FindPropertyRelative("handlerName");

            imageProperty.stringValue = library[element].guid.ToString();
            if (drawIndex[element] == 0)
                handlerNameProperty.stringValue = "";
            else
                handlerNameProperty.stringValue = possibleHandlerNames[drawIndex[element]];
        }

        // Get all the image target handlers
        List<string> GetAllHandlerNames()
        {
            possibleHandlerTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                .Where(x => typeof(IHandleImageTargets).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract).ToList();
            return possibleHandlerTypes.Select(x => x.Name).ToList();
        }

        // Get the System.ComponentModel Description attribute
        string GetDescription(Type handler)
        {
            if (handler != null)
            {
                var descriptions = (DescriptionAttribute[])handler.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (descriptions.Length != 0)
                    return descriptions[0].Description;
            }
            return null;
        }

        // Save the previous image reference library texture guids for comparison to a new reference image library
        void SavePreviousLibrary()
        {
            previousLibrary = new Dictionary<string, string>();
            foreach (var image in library)
            {
                if (!previousLibrary.ContainsKey(image.textureGuid.ToString()))
                {
                    previousLibrary.Add(image.textureGuid.ToString(), image.guid.ToString());
                }
            }
        }
    }
}