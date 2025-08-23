using UnityEngine;

namespace CatlikeCodings.ObjectManagement
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-23 11:57:07
    public abstract class SpawnZone : PersistableObject
    {
        [System.Serializable]
        public struct SpawnConfiguration
        {
            public enum MovementDirection
            {
                Forward,
                Upward,
                Outward,
                Random
            }

            public ShapeFactory[] factories;
            public MovementDirection movementDirection;
            public FloatRange speed;
            public FloatRange angularSpeed;
            public FloatRange scale;
            public ColorRangeHSV color;
            public bool uniformColor;
        }

        [SerializeField] private SpawnConfiguration spawnConfig;

        public abstract Vector3 SpawnPoint { get; }

        public virtual Shape SpawnShape()
        {
            var factoryIndex = Random.Range(0, spawnConfig.factories.Length);
            var shape = spawnConfig.factories[factoryIndex].GetRandom();

            var t = shape.transform;
            t.localPosition = SpawnPoint;
            t.localRotation = Random.rotation;
            t.localScale = Vector3.one * spawnConfig.scale.RandomValueInRange;
            if (spawnConfig.uniformColor)
            {
                shape.SetColor(spawnConfig.color.RandomInRange);
            }
            else
            {
                for (var i = 0; i < shape.ColorCount; i++)
                {
                    shape.SetColor(spawnConfig.color.RandomInRange, i);
                }
            }

            shape.AngularVelocity = Random.onUnitSphere * spawnConfig.angularSpeed.RandomValueInRange;
            var direction = spawnConfig.movementDirection switch
            {
                SpawnConfiguration.MovementDirection.Upward => transform.up,
                SpawnConfiguration.MovementDirection.Outward => (t.localPosition - transform.position).normalized,
                SpawnConfiguration.MovementDirection.Random => Random.onUnitSphere,
                _ => transform.forward
            };
            shape.Velocity = direction * spawnConfig.speed.RandomValueInRange;
            return shape;
        }
    }
}