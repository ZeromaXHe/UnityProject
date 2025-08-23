using UnityEditor;
using UnityEngine;

namespace CatlikeCodings.ObjectManagement.Editor
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-23 20:24:03
    public class GameLevelInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var gameLevel = (GameLevel)target;
            if (gameLevel.HasMissingLevelObjects)
            {
                EditorGUILayout.HelpBox("Missing level objects!", MessageType.Error);
                if (GUILayout.Button("Remove Missing Elements"))
                {
                    Undo.RecordObject(gameLevel, "Remove Missing Level Objects.");
                    gameLevel.RemoveMissingLevelObjects();
                }
            }
        }
    }
}