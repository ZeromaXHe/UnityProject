using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace CatlikeCodings.Prototypes.Maze2
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-22 16:21:11
    public class Agent : MonoBehaviour
    {
        [SerializeField] private string triggerMessage;
        [SerializeField] private bool isGoal;
        [SerializeField] private Color color = Color.white;
        [SerializeField, Min(0f)] private float speed = 1f;

        private Maze _maze;
        private int _targetIndex;
        private Vector3 _targetPosition;
        private Light _pointLight;

        public float LightRange => _pointLight.range;

        public string TriggerMessage => triggerMessage;

        private void Awake()
        {
            _pointLight = GetComponent<Light>();
            _pointLight.color = color;
            GetComponent<MeshRenderer>().material.color = color;
            var main = GetComponent<ParticleSystem>().main;
            main.startColor = color;
            gameObject.SetActive(false);
        }

        public void StartNewGame(Maze maze, int2 coordinates)
        {
            _maze = maze;
            _targetIndex = maze.CoordinatesToIndex(coordinates);
            _targetPosition = transform.localPosition =
                maze.CoordinatesToWorldPosition(coordinates, transform.localPosition.y);
            gameObject.SetActive(true);
        }

        public void EndGame() => gameObject.SetActive(false);

        private void Sniff(ref (int, float) trail, NativeArray<float> scent, int indexOffset)
        {
            var sniffIndex = _targetIndex + indexOffset;
            var detectedScent = scent[sniffIndex];
            if (isGoal ? detectedScent < trail.Item2 : detectedScent > trail.Item2)
            {
                trail = (sniffIndex, detectedScent);
            }
        }

        private bool TryFindNewTarget(NativeArray<float> scent)
        {
            var cell = _maze[_targetIndex];
            var trail = (0, isGoal ? float.MaxValue : 0f);

            if (cell.Has(MazeFlags.PassageNE))
            {
                Sniff(ref trail, scent, _maze.StepN + _maze.StepE);
            }

            if (cell.Has(MazeFlags.PassageNW))
            {
                Sniff(ref trail, scent, _maze.StepN + _maze.StepW);
            }

            if (cell.Has(MazeFlags.PassageSE))
            {
                Sniff(ref trail, scent, _maze.StepS + _maze.StepE);
            }

            if (cell.Has(MazeFlags.PassageSW))
            {
                Sniff(ref trail, scent, _maze.StepS + _maze.StepW);
            }

            if (cell.Has(MazeFlags.PassageE))
            {
                Sniff(ref trail, scent, _maze.StepE);
            }

            if (cell.Has(MazeFlags.PassageW))
            {
                Sniff(ref trail, scent, _maze.StepW);
            }

            if (cell.Has(MazeFlags.PassageN))
            {
                Sniff(ref trail, scent, _maze.StepN);
            }

            if (cell.Has(MazeFlags.PassageS))
            {
                Sniff(ref trail, scent, _maze.StepS);
            }

            if (trail.Item2 > 0f)
            {
                _targetIndex = trail.Item1;
                _targetPosition = _maze.IndexToWorldPosition(trail.Item1, _targetPosition.y);
                return true;
            }

            return false;
        }

        public Vector3 Move(NativeArray<float> scent)
        {
            var position = transform.localPosition;
            var targetVector = _targetPosition - position;
            var targetDistance = targetVector.magnitude;
            var movement = speed * Time.deltaTime;

            while (movement > targetDistance)
            {
                position = _targetPosition;
                if (TryFindNewTarget(scent))
                {
                    movement -= targetDistance;
                    targetVector = _targetPosition - position;
                    targetDistance = targetVector.magnitude;
                }
                else
                {
                    return transform.localPosition = position;
                }
            }

            return transform.localPosition = position + targetVector * (movement / targetDistance);
        }

        public void SetLightEnabled(bool enable) => _pointLight.enabled = enable;
    }
}