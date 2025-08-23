using UnityEngine;

namespace CatlikeCodings.ObjectManagement.Behaviors
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-23 17:57:33
    public sealed class SatelliteShapeBehavior : ShapeBehavior
    {
        private Vector3 _previousPosition;
        private ShapeInstance _focalShape;
        private float _frequency;
        private Vector3 _cosOffset, _sinOffset;

        public void Initialize(Shape shape, Shape focalShape, float radius, float frequency)
        {
            _focalShape = focalShape;
            _frequency = frequency;
            var orbitAxis = Random.onUnitSphere;
            do
            {
                _cosOffset = Vector3.Cross(orbitAxis, Random.onUnitSphere).normalized;
            } while (_cosOffset.sqrMagnitude < 0.1f);

            _sinOffset = Vector3.Cross(_cosOffset, orbitAxis);
            _cosOffset *= radius;
            _sinOffset *= radius;
            shape.AddBehavior<RotationShapeBehavior>().AngularVelocity =
                -360f * frequency * shape.transform.InverseTransformDirection(orbitAxis);
            GameUpdate(shape);
            _previousPosition = shape.transform.localPosition;
        }

        public override ShapeBehaviorType BehaviorType => ShapeBehaviorType.Satellite;

        public override bool GameUpdate(Shape shape)
        {
            if (_focalShape.IsValid)
            {
                var t = 2f * Mathf.PI * _frequency * shape.Age;
                _previousPosition = shape.transform.localPosition;
                shape.transform.localPosition =
                    _focalShape.Shape.transform.localPosition + _cosOffset * Mathf.Cos(t) + _sinOffset * Mathf.Sin(t);
                return true;
            }

            shape.AddBehavior<MovementShapeBehavior>().Velocity =
                (shape.transform.localPosition - _previousPosition) / Time.deltaTime;
            return false;
        }

        public override void Save(GameDataWriter writer)
        {
            writer.Write(_focalShape);
            writer.Write(_frequency);
            writer.Write(_cosOffset);
            writer.Write(_sinOffset);
            writer.Write(_previousPosition);
        }

        public override void Load(GameDataReader reader)
        {
            _focalShape = reader.ReadShapeInstance();
            _frequency = reader.ReadFloat();
            _cosOffset = reader.ReadVector3();
            _sinOffset = reader.ReadVector3();
            _previousPosition = reader.ReadVector3();
        }

        public override void Recycle()
        {
            ShapeBehaviorPool<SatelliteShapeBehavior>.Reclaim(this);
        }
        
        public override void ResolveShapeInstances () {
            _focalShape.Resolve();
        }
    }
}