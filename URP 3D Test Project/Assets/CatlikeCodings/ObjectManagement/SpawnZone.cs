using CatlikeCodings.ObjectManagement.Behaviors;
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
            public MovementDirection oscillationDirection;
            public FloatRange oscillationAmplitude;
            public FloatRange oscillationFrequency;

            [System.Serializable]
            public struct SatelliteConfiguration
            {
                public IntRange amount;
                [FloatRangeSlider(0.1f, 1f)] public FloatRange relativeScale;
                public FloatRange orbitRadius;
                public FloatRange orbitFrequency;
            }

            public SatelliteConfiguration satellite;
        }

        [SerializeField] private SpawnConfiguration spawnConfig;

        public abstract Vector3 SpawnPoint { get; }

        public virtual void SpawnShapes()
        {
            var factoryIndex = Random.Range(0, spawnConfig.factories.Length);
            var shape = spawnConfig.factories[factoryIndex].GetRandom();

            var t = shape.transform;
            t.localPosition = SpawnPoint;
            t.localRotation = Random.rotation;
            t.localScale = Vector3.one * spawnConfig.scale.RandomValueInRange;
            SetupColor(shape);
            var angularSpeed = spawnConfig.angularSpeed.RandomValueInRange;
            if (angularSpeed != 0f)
            {
                var rotation = shape.AddBehavior<RotationShapeBehavior>();
                rotation.AngularVelocity = Random.onUnitSphere * angularSpeed;
            }

            var speed = spawnConfig.speed.RandomValueInRange;
            if (speed != 0f)
            {
                var movement = shape.AddBehavior<MovementShapeBehavior>();
                movement.Velocity = GetDirectionVector(spawnConfig.movementDirection, t) * speed;
            }

            SetupOscillation(shape);
            var satelliteCount = spawnConfig.satellite.amount.RandomValueInRange;
            for (var i = 0; i < satelliteCount; i++)
            {
                CreateSatelliteFor(shape);
            }
        }

        private void SetupOscillation(Shape shape)
        {
            var amplitude = spawnConfig.oscillationAmplitude.RandomValueInRange;
            var frequency = spawnConfig.oscillationFrequency.RandomValueInRange;
            if (amplitude == 0f || frequency == 0f)
            {
                return;
            }

            var oscillation = shape.AddBehavior<OscillationShapeBehavior>();
            oscillation.Offset = GetDirectionVector(spawnConfig.oscillationDirection, shape.transform) * amplitude;
            oscillation.Frequency = frequency;
        }

        private Vector3 GetDirectionVector(SpawnConfiguration.MovementDirection direction, Transform t)
        {
            return direction switch
            {
                SpawnConfiguration.MovementDirection.Upward => transform.up,
                SpawnConfiguration.MovementDirection.Outward => (t.localPosition - transform.position).normalized,
                SpawnConfiguration.MovementDirection.Random => Random.onUnitSphere,
                _ => transform.forward
            };
        }

        private void CreateSatelliteFor(Shape focalShape)
        {
            var factoryIndex = Random.Range(0, spawnConfig.factories.Length);
            var shape = spawnConfig.factories[factoryIndex].GetRandom();
            var t = shape.transform;
            t.localRotation = Random.rotation;
            t.localScale = focalShape.transform.localScale * spawnConfig.satellite.relativeScale.RandomValueInRange;
            SetupColor(shape);
            shape.AddBehavior<SatelliteShapeBehavior>().Initialize(
                shape, focalShape,
                spawnConfig.satellite.orbitRadius.RandomValueInRange,
                spawnConfig.satellite.orbitFrequency.RandomValueInRange
            );
        }

        private void SetupColor(Shape shape)
        {
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
        }
    }
}