#define USE_UI_ELEMENTS
using System.Collections.Generic;
using UnityEngine.Reflect.Extensions.MaterialMapping;
#if USE_UI_ELEMENTS
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace UnityEditor.Reflect.Extensions.MaterialMapping
{
    internal static class ReflectEditorPreferences
    {
        public static bool autoExtractMaterialsOnImport
        {
            get => EditorPrefs.GetBool("Reflect:autoExtractMaterialsOnImport", false);
            internal set => EditorPrefs.SetBool("Reflect:autoExtractMaterialsOnImport", value);
        }

        public static string autoExtractRelativePath
        {
            get => EditorPrefs.GetString("Reflect:autoExtractRelativePath", "_remapped_materials");
            private set => EditorPrefs.SetString("Reflect:autoExtractRelativePath", value);
        }

        public static bool dontExtractRemappedMaterials
        {
            get => EditorPrefs.GetBool("Reflect:dontExtractRemappedMaterials", true);
            private set => EditorPrefs.SetBool("Reflect:dontExtractRemappedMaterials", value);
        }

        public static bool autoAssignRemapsOnExtract
        {
            get => EditorPrefs.GetBool("Reflect:autoAssignRemapsOnExtract", true);
            private set => EditorPrefs.SetBool("Reflect:autoAssignRemapsOnExtract", value);
        }

        public static bool convertExtractedMaterials
        {
            get => EditorPrefs.GetBool("Reflect:convertExtractedMaterials", false);
            private set => EditorPrefs.SetBool("Reflect:convertExtractedMaterials", value);
        }

        public static SyncPrefabScriptedImporterHelpers.MaterialConversion extractedMaterialsConverionMethod
        {
            get => (SyncPrefabScriptedImporterHelpers.MaterialConversion)EditorPrefs.GetInt("Reflect:extractedMaterialsConverionMethod", 0);
            private set => EditorPrefs.SetInt("Reflect:extractedMaterialsConverionMethod", (int)value);
        }

        public static MaterialsOverride.MatchType materialSearchMatchType
        {
            get => (MaterialsOverride.MatchType)EditorPrefs.GetInt("Reflect:materialSearchMatchType", 0);
            private set => EditorPrefs.SetInt("Reflect:materialSearchMatchType", (int)value);
        }

        [SettingsProvider]
        public static SettingsProvider ReflectSettingsProvider()
        {
            var provider = new SettingsProvider("Preferences/Reflect/Materials", SettingsScope.User) // TODO : move to Project Settings ?
            {
                label = "Materials",
#if !USE_UI_ELEMENTS
                guiHandler = (searchContext) =>
                {
                    GUILayout.Label("Extraction Settings");
                    autoExtractMaterialsOnImport = EditorGUILayout.Toggle("Auto Extract Materials", autoExtractMaterialsOnImport);
                    if (autoExtractMaterialsOnImport)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Auto Extract Relative Path");
                        autoExtractRelativePath = GUILayout.TextField(autoExtractRelativePath);
                        GUILayout.EndHorizontal();
                    }
                    dontExtractRemappedMaterials = EditorGUILayout.Toggle("Don't Extract Remapped Materials", dontExtractRemappedMaterials);
                    convertExtractedMaterials = EditorGUILayout.Toggle("Convert Extracted Materials", convertExtractedMaterials);
                    if (convertExtractedMaterials)
                    {
                        extractedMaterialsConverionMethod = (SyncPrefabScriptedImporterHelpers.MaterialConversion)EditorGUILayout.EnumPopup("Conversion Method", extractedMaterialsConverionMethod);
                    }
                    autoAssignRemapsOnExtract = EditorGUILayout.Toggle("Assign Remaps after Extraction", autoAssignRemapsOnExtract);
                },
#else
                activateHandler = (searchContext, rootElement) =>
                {
                    var title = new Label()
                    {
                        text = "Extraction Settings"
                    };
                    title.AddToClassList("title");
                    rootElement.Add(title);

                    var properties = new VisualElement()
                    {
                        style =
                        {
                            flexDirection = FlexDirection.Column
                        }
                    };
                    properties.AddToClassList("property-list");
                    rootElement.Add(properties);

                    // SKIP REMAPPED MATERIALS
                    var dontExtractRemappedMats_tgl = new Toggle()
                    {
                        label = "Skip Remapped Materials",
                        value = dontExtractRemappedMaterials
                    };
                    dontExtractRemappedMats_tgl.RegisterValueChangedCallback(v => dontExtractRemappedMaterials = v.newValue);
                    dontExtractRemappedMats_tgl.AddToClassList("property-value");

                    // MATERIAL CONVERSION METHOD
                    var materialConversion_sel = new EnumField(extractedMaterialsConverionMethod)
                    {
                        label = "Material Conversion Method",
                        visible = convertExtractedMaterials
                    };
                    materialConversion_sel.RegisterValueChangedCallback(v => extractedMaterialsConverionMethod = (SyncPrefabScriptedImporterHelpers.MaterialConversion)v.newValue);
                    materialConversion_sel.AddToClassList("property-value");

                    // CONVERT EXTRACTED MATERIALS
                    var convertExtractedMaterials_tgl = new Toggle()
                    {
                        label = "Convert Extracted Materials",
                        value = convertExtractedMaterials
                    };
                    convertExtractedMaterials_tgl.RegisterValueChangedCallback(v => { convertExtractedMaterials = v.newValue; materialConversion_sel.SetEnabled(v.newValue); });
                    convertExtractedMaterials_tgl.AddToClassList("property-value");

                    // ASSIGN REMAPS
                    var assignRemaps_tgl = new Toggle()
                    {
                        label = "Assign Material Remaps",
                        value = autoAssignRemapsOnExtract
                    };
                    assignRemaps_tgl.RegisterValueChangedCallback(v => autoAssignRemapsOnExtract = v.newValue);
                    assignRemaps_tgl.AddToClassList("property-value");

                    // AUTO EXTRACT RELATIVE PATH
                    var autoExtractPath_tf = new TextField()
                    {
                        label = "Auto Extract Materials to",
                        value = autoExtractRelativePath
                    };
                    autoExtractPath_tf.RegisterValueChangedCallback(v => autoExtractRelativePath = v.newValue);
                    autoExtractPath_tf.AddToClassList("property-value");

                    // AUTO EXTRACT ON IMPORT
                    var autoExtract_tgl = new Toggle()
                    {
                        label = "Auto Extract Materials on Import",
                        value = autoExtractMaterialsOnImport
                    };
                    autoExtract_tgl.RegisterValueChangedCallback(v => { autoExtractMaterialsOnImport = v.newValue; autoExtractPath_tf.SetEnabled(v.newValue); });
                    autoExtract_tgl.AddToClassList("property-value");

                    // MATERIAL SEARCH MATCH TYPE
                    var materialSearchMatchType_sel = new EnumField(materialSearchMatchType)
                    {
                        label = "Material Search Match Type",
                        tooltip = "Method to match found Materials Names(A) with Mapping Names(B)."
                    };
                    materialSearchMatchType_sel.RegisterValueChangedCallback(v => materialSearchMatchType = (MaterialsOverride.MatchType)v.newValue);
                    materialSearchMatchType_sel.AddToClassList("property-value");

                    // ADDING PROPERTIES
                    properties.Add(dontExtractRemappedMats_tgl);
                    properties.Add(convertExtractedMaterials_tgl);
                    properties.Add(materialConversion_sel);
                    materialConversion_sel.SetEnabled(convertExtractedMaterials);
                    properties.Add(assignRemaps_tgl);
                    properties.Add(autoExtract_tgl);
                    properties.Add(autoExtractPath_tf);
                    autoExtractPath_tf.SetEnabled(autoExtractMaterialsOnImport);
                    properties.Add(materialSearchMatchType_sel);
                },
#endif
                keywords = new HashSet<string>(new[] { "Material", "Mapping" })
            };
            return provider;
        }
    }
}