using UnityEditor;
using UnityEngine;

namespace CatlikeCodings.ObjectManagement.Editor
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-23 14:57:46
    [CustomPropertyDrawer(typeof(FloatRange))]
    public class FloatRangeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var originalIndentLevel = EditorGUI.indentLevel;
            var originalLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            position.width = position.width / 2f;
            EditorGUIUtility.labelWidth = position.width / 2f;
            EditorGUI.indentLevel = 1;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("min"));
            position.x += position.width;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("max"));
            EditorGUI.EndProperty();
            EditorGUI.indentLevel = originalIndentLevel;
            EditorGUIUtility.labelWidth = originalLabelWidth;
        }
    }
}