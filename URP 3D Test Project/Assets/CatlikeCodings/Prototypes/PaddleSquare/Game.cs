using TMPro;
using UnityEngine;

namespace CatlikeCodings.Prototypes.PaddleSquare
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-15 16:38:59
    public class Game : MonoBehaviour
    {
        [SerializeField, Min(2)] private int pointsToWin = 3;
        [SerializeField] private LivelyCamera livelyCamera;
        [SerializeField] private Ball ball;
        [SerializeField] private Paddle bottomPaddle, topPaddle;
        [SerializeField, Min(0f)] private Vector2 arenaExtents = new(10f, 10f);
        [SerializeField] private TextMeshPro countdownText;
        [SerializeField, Min(1f)] private float newGameDelay = 3f;
        private float _countdownUntilNewGame;

        private void Awake() => _countdownUntilNewGame = newGameDelay;

        private void StartNewGame()
        {
            ball.StartNewGame();
            bottomPaddle.StartNewGame();
            topPaddle.StartNewGame();
        }

        private void Update()
        {
            bottomPaddle.Move(ball.Position.x, arenaExtents.x);
            topPaddle.Move(ball.Position.x, arenaExtents.x);
            if (_countdownUntilNewGame <= 0f)
            {
                UpdateGame();
            }
            else
            {
                UpdateCountdown();
            }
        }

        private void UpdateGame()
        {
            ball.Move();
            BounceYIfNeeded();
            BounceXIfNeeded(ball.Position.x);
            ball.UpdateVisualization();
        }

        private void UpdateCountdown()
        {
            _countdownUntilNewGame -= Time.deltaTime;
            if (_countdownUntilNewGame <= 0f)
            {
                countdownText.gameObject.SetActive(false);
                StartNewGame();
            }
            else
            {
                var displayValue = Mathf.Ceil(_countdownUntilNewGame);
                if (displayValue < newGameDelay)
                {
                    countdownText.SetText("{0}", displayValue);
                }
            }
        }

        private void BounceXIfNeeded(float x)
        {
            var xExtents = arenaExtents.x - ball.Extents;
            if (x < -xExtents)
            {
                livelyCamera.PushXZ(ball.Velocity);
                ball.BounceX(-xExtents);
            }
            else if (x > xExtents)
            {
                livelyCamera.PushXZ(ball.Velocity);
                ball.BounceX(xExtents);
            }
        }

        private void BounceYIfNeeded()
        {
            var yExtents = arenaExtents.y - ball.Extents;
            if (ball.Position.y < -yExtents)
            {
                BounceY(-yExtents, bottomPaddle, topPaddle);
            }
            else if (ball.Position.y > yExtents)
            {
                BounceY(yExtents, topPaddle, bottomPaddle);
            }
        }

        private void BounceY(float boundary, Paddle defender, Paddle attacker)
        {
            var durationAfterBounce = (ball.Position.y - boundary) / ball.Velocity.y;
            var bounceX = ball.Position.x - ball.Velocity.x * durationAfterBounce;
            BounceXIfNeeded(bounceX);
            bounceX = ball.Position.x - ball.Velocity.x * durationAfterBounce;
            livelyCamera.PushXZ(ball.Velocity);
            ball.BounceY(boundary);
            if (defender.HitBall(bounceX, ball.Extents, out var hitFactor))
            {
                ball.SetXPositionAndSpeed(bounceX, hitFactor, durationAfterBounce);
            }
            else
            {
                livelyCamera.JostleY();
                if (attacker.ScorePoint(pointsToWin))
                {
                    EndGame();
                }
            }
        }

        private void EndGame()
        {
            _countdownUntilNewGame = newGameDelay;
            countdownText.SetText("GAME OVER");
            countdownText.gameObject.SetActive(true);
            ball.EndGame();
        }
    }
}