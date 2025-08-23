#if UNITY_EDITOR
using System.Linq;
using UnityEngine;

namespace CatlikeCodings.ObjectManagement
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-23 12:07:35
    public partial class GameLevel
    {
        public bool HasMissingLevelObjects =>
            levelObjects != null && levelObjects.Any(obj => obj == null);

        public void RemoveMissingLevelObjects()
        {
            if (Application.isPlaying)
            {
                Debug.LogError("Do not invoke in play mode!");
                return;
            }

            var holes = 0;
            for (var i = 0; i < levelObjects.Length - holes; i++)
            {
                if (levelObjects[i] == null)
                {
                    holes += 1;
                    System.Array.Copy(
                        levelObjects, i + 1, levelObjects, i,
                        levelObjects.Length - i - holes
                    );
                    i -= 1;
                }
            }

            System.Array.Resize(ref levelObjects, levelObjects.Length - holes);
        }

        public bool HasLevelObject(GameLevelObject o) =>
            levelObjects != null && levelObjects.Any(obj => obj == o);

        public void RegisterLevelObject(GameLevelObject o)
        {
            if (Application.isPlaying)
            {
                Debug.LogError("Do not invoke in play mode!");
                return;
            }

            if (HasLevelObject(o))
            {
                return;
            }

            if (levelObjects == null)
            {
                levelObjects = new[] { o };
            }
            else
            {
                System.Array.Resize(ref levelObjects, levelObjects.Length + 1);
                levelObjects[^1] = o;
            }
        }
    }
}
#endif