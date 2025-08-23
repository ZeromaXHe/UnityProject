using UnityEngine;

namespace CatlikeCodings.ObjectManagement.Behaviors
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-23 18:54:02
    public sealed class LifecycleShapeBehavior : ShapeBehavior
    {
        private float adultDuration, dyingDuration, _dyingAge;

        public void Initialize(Shape shape, float growingDuration, float adultDuration, float dyingDuration)
        {
            this.adultDuration = adultDuration;
            this.dyingDuration = dyingDuration;
            _dyingAge = growingDuration + adultDuration;

            if (growingDuration > 0f)
            {
                shape.AddBehavior<GrowingShapeBehavior>().Initialize(
                    shape, growingDuration
                );
            }
        }

        public override ShapeBehaviorType BehaviorType => ShapeBehaviorType.Growing;

        public override bool GameUpdate(Shape shape)
        {
            if (shape.Age >= _dyingAge)
            {
                if (dyingDuration <= 0f)
                {
                    shape.Die();
                    return true;
                }

                if (!shape.IsMarkedAsDying)
                {
                    shape.AddBehavior<DyingShapeBehavior>().Initialize(shape, dyingDuration + _dyingAge - shape.Age);
                }

                return false;
            }

            return true;
        }

        public override void Save(GameDataWriter writer)
        {
            writer.Write(adultDuration);
            writer.Write(dyingDuration);
            writer.Write(_dyingAge);
        }

        public override void Load(GameDataReader reader)
        {
            adultDuration = reader.ReadFloat();
            dyingDuration = reader.ReadFloat();
            _dyingAge = reader.ReadFloat();
        }

        public override void Recycle()
        {
            ShapeBehaviorPool<LifecycleShapeBehavior>.Reclaim(this);
        }
    }
}