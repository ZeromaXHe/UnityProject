using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CatlikeCodings.ObjectManagement.Editor
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-23 20:29:11
    static class RegisterLevelObjectMenuItem
    {
        const string menuItem = "GameObject/Register Level Object";

        [MenuItem(menuItem, true)]
        private static bool ValidateRegisterLevelObject() =>
            Selection.objects.Length != 0 && Selection.objects.All(o => o is GameObject);

        [MenuItem(menuItem)]
        private static void RegisterLevelObject()
        {
            foreach (var o in Selection.objects)
            {
                Register(o as GameObject);
            }
        }

        private static void Register(GameObject o)
        {
            if (PrefabUtility.GetPrefabType(o) == PrefabType.Prefab)
            {
                Debug.LogWarning(o.name + " is a prefab asset.", o);
                return;
            }

            var levelObject = o.GetComponent<GameLevelObject>();
            if (levelObject == null)
            {
                Debug.LogWarning(o.name + " isn't a game level object.", o);
                return;
            }

            foreach (var rootObject in o.scene.GetRootGameObjects())
            {
                var gameLevel = rootObject.GetComponent<GameLevel>();
                if (gameLevel != null)
                {
                    if (gameLevel.HasLevelObject(levelObject))
                    {
                        Debug.LogWarning(o.name + " is already registered.", o);
                        return;
                    }

                    Undo.RecordObject(gameLevel, "Register Level Object.");
                    gameLevel.RegisterLevelObject(levelObject);
                    Debug.Log(
                        o.name + " registered to game level " +
                        gameLevel.name + " in scene " + o.scene.name + ".", o
                    );
                    return;
                }
            }

            Debug.LogWarning(o.name + " isn't part of a game level.", o);
        }
    }
}