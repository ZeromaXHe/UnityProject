using UnityEngine;

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

        public Shape Get(int shapeId = 0, int materialId = 0)
        {
            var instance = Instantiate(prefabs[shapeId]);
            instance.ShapeId = shapeId;
            instance.SetMaterial(materials[materialId], materialId);
            return instance;
        }

        public Shape GetRandom() =>
            Get(Random.Range(0, prefabs.Length), Random.Range(0, materials.Length));
    }
}