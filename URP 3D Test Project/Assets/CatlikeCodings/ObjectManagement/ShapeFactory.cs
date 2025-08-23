using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CatlikeCodings.ObjectManagement
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-22 22:46:39
    [CreateAssetMenu]
    public class ShapeFactory : ScriptableObject
    {
        [SerializeField] private Shape[] prefabs;
        [SerializeField] private Material[] materials;
        [SerializeField] private bool recycle;

        private Scene _poolScene;
        private List<Shape>[] _pools;

        // [MemberNotNull(nameof(_pools))] // 不知道为啥导不了这个的包 System.Diagnostics.CodeAnalysis，NotNull 就是正常的
        private void CreatePools()
        {
            _pools = new List<Shape>[prefabs.Length];
            for (var i = 0; i < _pools.Length; i++)
            {
                _pools[i] = new List<Shape>();
            }

            if (Application.isEditor)
            {
                _poolScene = SceneManager.GetSceneByName(name);
                if (_poolScene.isLoaded)
                {
                    var rootObjects = _poolScene.GetRootGameObjects();
                    foreach (var rootObj in rootObjects)
                    {
                        var pooledShape = rootObj.GetComponent<Shape>();
                        if (!pooledShape.gameObject.activeSelf)
                        {
                            _pools[pooledShape.ShapeId].Add(pooledShape);
                        }
                    }

                    return;
                }
            }

            _poolScene = SceneManager.CreateScene(name);
        }

        public Shape Get(int shapeId = 0, int materialId = 0)
        {
            Shape instance;
            if (recycle)
            {
                if (_pools == null)
                {
                    CreatePools();
                }

                var pool = _pools[shapeId];
                var lastIndex = pool.Count - 1;
                if (lastIndex >= 0)
                {
                    instance = pool[lastIndex];
                    instance.gameObject.SetActive(true);
                    pool.RemoveAt(lastIndex);
                }
                else
                {
                    instance = Instantiate(prefabs[shapeId]);
                    instance.ShapeId = shapeId;
                    SceneManager.MoveGameObjectToScene(instance.gameObject, _poolScene);
                }
            }
            else
            {
                instance = Instantiate(prefabs[shapeId]);
                instance.ShapeId = shapeId;
            }

            instance.SetMaterial(materials[materialId], materialId);
            return instance;
        }

        public void Reclaim(Shape shapeToRecycle)
        {
            if (recycle)
            {
                if (_pools == null)
                {
                    CreatePools();
                }

                _pools[shapeToRecycle.ShapeId].Add(shapeToRecycle);
                shapeToRecycle.gameObject.SetActive(false);
            }
            else
            {
                Destroy(shapeToRecycle.gameObject);
            }
        }

        public Shape GetRandom() =>
            Get(Random.Range(0, prefabs.Length), Random.Range(0, materials.Length));
    }
}