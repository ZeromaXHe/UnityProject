using System;
using UnityEngine;

namespace CatlikeCodings.ObjectManagement.Behaviors
{
    public enum ShapeBehaviorType
    {
        Movement,
        Rotation,
        Oscillation,
        Satellite
    }

    public static class ShapeBehaviorTypeMethods
    {
        public static ShapeBehavior GetInstance(this ShapeBehaviorType type)
        {
            switch (type)
            {
                case ShapeBehaviorType.Movement:
                    return ShapeBehaviorPool<MovementShapeBehavior>.Get();
                case ShapeBehaviorType.Rotation:
                    return ShapeBehaviorPool<RotationShapeBehavior>.Get();
                case ShapeBehaviorType.Oscillation:
                    return ShapeBehaviorPool<OscillationShapeBehavior>.Get();
                case ShapeBehaviorType.Satellite:
                    return ShapeBehaviorPool<SatelliteShapeBehavior>.Get();
                default:
                    Debug.Log("Forgot to support " + type);
                    return null;
            }
        }
    }

    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-23 16:04:16
    public abstract class ShapeBehavior
#if UNITY_EDITOR
        : ScriptableObject
#endif
    {
#if UNITY_EDITOR
        public bool IsReclaimed { get; set; }

        private void OnEnable()
        {
            if (IsReclaimed)
            {
                Recycle();
            }
        }
#endif

        public virtual void ResolveShapeInstances()
        {
        }

        public abstract bool GameUpdate(Shape shape);
        public abstract void Save(GameDataWriter writer);
        public abstract void Load(GameDataReader reader);
        public abstract ShapeBehaviorType BehaviorType { get; }
        public abstract void Recycle();
    }
}