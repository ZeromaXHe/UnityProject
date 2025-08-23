using UnityEngine;

namespace CatlikeCodings.ObjectManagement.Behaviors
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-23 16:25:09
    public sealed class OscillationShapeBehavior : ShapeBehavior
    {
        public Vector3 Offset { get; set; }
        public float Frequency { get; set; }
        private float _previousOscillation;
        public override ShapeBehaviorType BehaviorType => ShapeBehaviorType.Oscillation;

        public override void GameUpdate(Shape shape)
        {
            var oscillation = Mathf.Sin(2f * Mathf.PI * Frequency * shape.Age);
            shape.transform.localPosition += (oscillation - _previousOscillation) * Offset;
            _previousOscillation = oscillation;
        }

        public override void Save(GameDataWriter writer)
        {
            writer.Write(Offset);
            writer.Write(Frequency);
            writer.Write(_previousOscillation);
        }

        public override void Load(GameDataReader reader)
        {
            Offset = reader.ReadVector3();
            Frequency = reader.ReadFloat();
            _previousOscillation = reader.ReadFloat();
        }

        public override void Recycle()
        {
            _previousOscillation = 0f;
            ShapeBehaviorPool<OscillationShapeBehavior>.Reclaim(this);
        }
    }
}