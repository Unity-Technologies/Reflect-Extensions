using UnityEngine;
using UnityEngine.Reflect.Extensions.Timeline;
using Unity.SunStudy;
using System;

namespace UnityEditor.Reflect.Extensions.Timeline
{
    [CustomPropertyDrawer(typeof(SunStudyBehaviour))]
    public class SunStudyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 5 * EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty labelProp = property.FindPropertyRelative("label");
            SerializedProperty yearProp = property.FindPropertyRelative("year");
            SerializedProperty dayProp = property.FindPropertyRelative("dayOfYear");
            SerializedProperty minuteProp = property.FindPropertyRelative("minuteOfDay");

            EditorGUILayout.PropertyField(labelProp);
            EditorGUILayout.PropertyField(yearProp);
            EditorGUILayout.Slider(dayProp, 0, 365);
            EditorGUILayout.Slider(minuteProp, 0, 1440);

            var dayOfyear = SunStudy.SetDayOfYear(yearProp.intValue, (int)dayProp.floatValue);
            var timeOfDay = SunStudy.SetMinuteOfDay((int)minuteProp.floatValue);
            var date = new DateTime(yearProp.intValue, dayOfyear.month, dayOfyear.day, timeOfDay.hour, timeOfDay.minute, 0);
            EditorGUILayout.HelpBox(date.ToString("MMMM dd yyyy h:mm tt"), MessageType.None);
        }
    }
}