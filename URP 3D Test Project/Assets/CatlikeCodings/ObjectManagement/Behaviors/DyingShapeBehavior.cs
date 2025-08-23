using UnityEngine;

namespace CatlikeCodings.ObjectManagement.Behaviors
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-23 18:54:02
    public sealed class DyingShapeBehavior : ShapeBehavior
    {
        private Vector3 _originalScale;
        private float _duration, _dyingAge;

        public void Initialize(Shape shape, float duration)
        {
            _originalScale = shape.transform.localScale;
            _duration = duration;
            _dyingAge = shape.Age;
            shape.MarkAsDying();
        }

        public override ShapeBehaviorType BehaviorType => ShapeBehaviorType.Growing;

        public override bool GameUpdate(Shape shape)
        {
            var dyingDuration = shape.Age - _dyingAge;
            if (dyingDuration < _duration)
            {
                var s = 1f - dyingDuration / _duration;
                s = (3f - 2f * s) * s * s;
                shape.transform.localScale = s * _originalScale;
                return true;
            }

            shape.Die();
            return true;
        }

        public override void Save(GameDataWriter writer)
        {
            writer.Write(_originalScale);
            writer.Write(_duration);
            writer.Write(_dyingAge);
        }

        public override void Load(GameDataReader reader)
        {
            _originalScale = reader.ReadVector3();
            _duration = reader.ReadFloat();
            _dyingAge = reader.ReadFloat();
        }

        public override void Recycle()
        {
            ShapeBehaviorPool<DyingShapeBehavior>.Reclaim(this);
        }
    }
}