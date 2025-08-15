using UnityEngine;

namespace CatlikeCodings.Prototypes.PaddleSquare
{
    public class Ball : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float maxXSpeed = 20f,
            maxStartXSpeed = 2f,
            constantYSpeed = 10f,
            extents = 0.5f;

        [SerializeField] private ParticleSystem bounceParticleSystem, startParticleSystem, trailParticleSystem;

        [SerializeField] private int bounceParticleEmission = 20,
            startParticleEmission = 100;

        private Vector2 _position, _velocity;
        public float Extents => extents;
        public Vector2 Position => _position;
        public Vector2 Velocity => _velocity;

        public void UpdateVisualization() =>
            trailParticleSystem.transform.localPosition =
                transform.localPosition = new Vector3(_position.x, 0f, _position.y);

        public void Move() => _position += _velocity * Time.deltaTime;

        private void Awake() => gameObject.SetActive(false);

        public void StartNewGame()
        {
            _position = Vector2.zero;
            UpdateVisualization();
            _velocity.x = Random.Range(-maxStartXSpeed, maxStartXSpeed);
            _velocity.y = -constantYSpeed;
            gameObject.SetActive(true);
            startParticleSystem.Emit(startParticleEmission);
            SetTrailEmission(true);
            trailParticleSystem.Play();
        }

        public void EndGame()
        {
            _position.x = 0f;
            gameObject.SetActive(false);
            SetTrailEmission(false);
        }

        private void SetTrailEmission (bool enable)
        {
            var emission = trailParticleSystem.emission;
            emission.enabled = enable;
        }

        public void SetXPositionAndSpeed(float start, float speedFactor, float deltaTime)
        {
            _velocity.x = maxXSpeed * speedFactor;
            _position.x = start + _velocity.x * deltaTime;
        }

        public void BounceX(float boundary)
        {
            var durationAfterBounce = (_position.x - boundary) / _velocity.x;
            _position.x = 2f * boundary - _position.x;
            _velocity.x = -_velocity.x;
            EmitBounceParticles(
                boundary,
                _position.y - _velocity.y * durationAfterBounce,
                boundary < 0f ? 90f : 270f
            );
        }

        public void BounceY(float boundary)
        {
            var durationAfterBounce = (_position.y - boundary) / _velocity.y;
            _position.y = 2f * boundary - _position.y;
            _velocity.y = -_velocity.y;
            EmitBounceParticles(
                _position.x - _velocity.x * durationAfterBounce,
                boundary,
                boundary < 0f ? 0f : 180f
            );
        }

        private void EmitBounceParticles(float x, float z, float rotation)
        {
            var shape = bounceParticleSystem.shape;
            shape.position = new Vector3(x, 0f, z);
            shape.rotation = new Vector3(0f, rotation, 0f);
            bounceParticleSystem.Emit(bounceParticleEmission);
        }
    }
}