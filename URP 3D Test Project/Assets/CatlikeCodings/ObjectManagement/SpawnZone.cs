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

            public MovementDirection movementDirection;
            public FloatRange speed;
            public FloatRange angularSpeed;
            public FloatRange scale;
            public ColorRangeHSV color;
        }

        [SerializeField] private SpawnConfiguration spawnConfig;

        public abstract Vector3 SpawnPoint { get; }

        public virtual void ConfigureSpawn(Shape shape)
        {
            var t = shape.transform;
            t.localPosition = SpawnPoint;
            t.localRotation = Random.rotation;
            t.localScale = Vector3.one * spawnConfig.scale.RandomValueInRange;
            shape.SetColor(spawnConfig.color.RandomInRange);
            shape.AngularVelocity = Random.onUnitSphere * spawnConfig.angularSpeed.RandomValueInRange;
            var direction = spawnConfig.movementDirection switch
            {
                SpawnConfiguration.MovementDirection.Upward => transform.up,
                SpawnConfiguration.MovementDirection.Outward => (t.localPosition - transform.position).normalized,
                SpawnConfiguration.MovementDirection.Random => Random.onUnitSphere,
                _ => transform.forward
            };
            shape.Velocity = direction * spawnConfig.speed.RandomValueInRange;
        }
    }
}