using UnityEngine;

namespace CatlikeCodings.Prototypes.PaddleSquare
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-15 18:13:33
    public class LivelyCamera : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float springStrength = 100f,
            dampingStrength = 10f,
            jostleStrength = 40f,
            pushStrength = 1f,
            maxDeltaTime = 1f / 60f;

        private Vector3 _anchorPosition, _velocity;

        private void Awake () => _anchorPosition = transform.localPosition;
        
        public void JostleY() => _velocity.y += jostleStrength;

        public void PushXZ(Vector2 impulse)
        {
            _velocity.x += pushStrength * impulse.x;
            _velocity.z += pushStrength * impulse.y;
        }

        private void LateUpdate()
        {
            var dt = Time.deltaTime;
            while (dt > maxDeltaTime)
            {
                TimeStep(maxDeltaTime);
                dt -= maxDeltaTime;
            }
            TimeStep(dt);
        }

        private void TimeStep (float dt)
        {
            var displacement = _anchorPosition - transform.localPosition;
            var acceleration = springStrength * displacement - dampingStrength * _velocity;
            _velocity += acceleration * dt;
            transform.localPosition += _velocity * dt;
        }
    }
}