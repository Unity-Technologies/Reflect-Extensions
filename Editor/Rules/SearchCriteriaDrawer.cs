using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine.Reflect.Extensions.Rules;

namespace UnityEditor.Reflect.Extensions.Rules
{
    [CustomPropertyDrawer(typeof(SearchCriteria))]
    public class SearchCriteriaDrawer : PropertyDrawer
    {
        //public override VisualElement CreatePropertyGUI(SerializedProperty property)
        //{
        //    // Create property container element.
        //    var container = new VisualElement();
        //    container.Add(new UIElements.ColorField("TEST"));

        //    // Create property fields.
        //    var keyField = new PropertyField(property.FindPropertyRelative("key"));
        //    var valueField = new PropertyField(property.FindPropertyRelative("value"));

        //    // Add fields to the container.
        //    container.Add(keyField);
        //    container.Add(valueField);

        //    return container;
        //}

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            //position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), new GUIContent("¬"));

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects
            var halfWidth = position.width * .5f;
            var keyRect = new Rect(position.x, position.y, halfWidth, position.height);
            var valueRect = new Rect(position.x + halfWidth, position.y, halfWidth, position.height);

            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            EditorGUI.PropertyField(keyRect, property.FindPropertyRelative("key"), GUIContent.none);
            EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("value"), GUIContent.none);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}