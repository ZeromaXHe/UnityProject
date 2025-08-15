using TMPro;
using UnityEngine;

namespace CatlikeCodings.Prototypes.PaddleSquare
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-15 16:38:26
    public class Paddle : MonoBehaviour
    {
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor"),
            FaceColorId = Shader.PropertyToID("_FaceColor"),
            TimeOfLastHitId = Shader.PropertyToID("_TimeOfLastHit");

        [SerializeField] private TextMeshPro scoreText;
        [SerializeField] private MeshRenderer goalRenderer;

        [SerializeField, ColorUsage(true, true)]
        private Color goalColor = Color.white;

        [SerializeField, Min(0f)] private float minExtents = 4f,
            maxExtents = 4f,
            speed = 10f,
            maxTargetingBias = 0.75f;

        [SerializeField] private bool isAI;
        private int _score;
        private float _extents, _targetingBias;
        private Material _goalMaterial, _paddleMaterial, _scoreMaterial;

        private void Awake()
        {
            _goalMaterial = goalRenderer.material;
            _goalMaterial.SetColor(EmissionColorId, goalColor);
            _paddleMaterial = GetComponent<MeshRenderer>().material;
            _scoreMaterial = scoreText.fontMaterial;
            SetScore(0);
        }

        public void Move(float target, float arenaExtents)
        {
            var p = transform.localPosition;
            p.x = isAI ? AdjustByAI(p.x, target) : AdjustByPlayer(p.x);
            var limit = arenaExtents - _extents;
            p.x = Mathf.Clamp(p.x, -limit, limit);
            transform.localPosition = p;
        }

        private float AdjustByAI(float x, float target)
        {
            target += _targetingBias * _extents;
            return x < target
                ? Mathf.Min(x + speed * Time.deltaTime, target)
                : Mathf.Max(x - speed * Time.deltaTime, target);
        }

        private float AdjustByPlayer(float x)
        {
            var goRight = Input.GetKey(KeyCode.RightArrow);
            var goLeft = Input.GetKey(KeyCode.LeftArrow);
            if (goRight && !goLeft)
            {
                return x + speed * Time.deltaTime;
            }

            if (goLeft && !goRight)
            {
                return x - speed * Time.deltaTime;
            }

            return x;
        }

        public bool HitBall(float ballX, float ballExtents, out float hitFactor)
        {
            ChangeTargetingBias();
            hitFactor = (ballX - transform.localPosition.x) / (_extents + ballExtents);
            var success = hitFactor is >= -1f and <= 1f;
            if (success)
            {
                _paddleMaterial.SetFloat(TimeOfLastHitId, Time.time);
            }

            return success;
        }

        public void StartNewGame()
        {
            SetScore(0);
            ChangeTargetingBias();
        }

        public bool ScorePoint(int pointsToWin)
        {
            _goalMaterial.SetFloat(TimeOfLastHitId, Time.time);
            SetScore(_score + 1, pointsToWin);
            return _score >= pointsToWin;
        }

        private void SetScore(int newScore, float pointsToWin = 1000f)
        {
            _score = newScore;
            scoreText.SetText("{0}", newScore);
            _scoreMaterial.SetColor(FaceColorId, goalColor * (newScore / pointsToWin));
            SetExtents(Mathf.Lerp(maxExtents, minExtents, newScore / (pointsToWin - 1f)));
        }

        private void ChangeTargetingBias() => _targetingBias = Random.Range(-maxTargetingBias, maxTargetingBias);

        private void SetExtents(float newExtents)
        {
            _extents = newExtents;
            var s = transform.localScale;
            s.x = 2f * newExtents;
            transform.localScale = s;
        }
    }
}