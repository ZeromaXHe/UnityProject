using UnityEditor;
using UnityEngine;

namespace CatlikeCodings.ObjectManagement.Editor
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-23 15:05:35
    [CustomPropertyDrawer(typeof(FloatRangeSliderAttribute))]
    public class FloatRangeSliderDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var originalIndentLevel = EditorGUI.indentLevel;
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(
                position, GUIUtility.GetControlID(FocusType.Passive), label
            );
            EditorGUI.indentLevel = 0;
            var minProperty = property.FindPropertyRelative("min");
            var maxProperty = property.FindPropertyRelative("max");
            var minValue = minProperty.floatValue;
            var maxValue = maxProperty.floatValue;
            var fieldWidth = position.width / 4f - 4f;
            var sliderWidth = position.width / 2f;
            position.width = fieldWidth;
            minValue = EditorGUI.FloatField(position, minValue);
            position.x += fieldWidth + 4f;
            position.width = sliderWidth;
            var limit = attribute as FloatRangeSliderAttribute;
            EditorGUI.MinMaxSlider(position, ref minValue, ref maxValue, limit.Min, limit.Max);
            position.x += sliderWidth + 4f;
            position.width = fieldWidth;
            maxValue = EditorGUI.FloatField(position, maxValue);
            if (minValue < limit.Min)
            {
                minValue = limit.Min;
            }
            else if (minValue > limit.Max)
            {
                minValue = limit.Max;
            }

            if (maxValue < minValue)
            {
                maxValue = minValue;
            }
            else if (maxValue > limit.Max)
            {
                maxValue = limit.Max;
            }

            minProperty.floatValue = minValue;
            maxProperty.floatValue = maxValue;
            EditorGUI.EndProperty();
            EditorGUI.indentLevel = originalIndentLevel;
        }
    }
}