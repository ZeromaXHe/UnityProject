using UnityEngine;

namespace CatlikeCodings.ObjectManagement.Behaviors
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-23 16:07:17
    public sealed class RotationShapeBehavior : ShapeBehavior
    {
        public Vector3 AngularVelocity { get; set; }

        public override bool GameUpdate(Shape shape)
        {
            shape.transform.Rotate(AngularVelocity * Time.deltaTime);
            return true;
        }

        public override void Save(GameDataWriter writer)
        {
            writer.Write(AngularVelocity);
        }

        public override void Load(GameDataReader reader)
        {
            AngularVelocity = reader.ReadVector3();
        }

        public override ShapeBehaviorType BehaviorType => ShapeBehaviorType.Rotation;

        public override void Recycle()
        {
            ShapeBehaviorPool<RotationShapeBehavior>.Reclaim(this);
        }
    }
}