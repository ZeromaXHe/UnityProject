using TMPro;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;
using Random = UnityEngine.Random;

namespace CatlikeCodings.Prototypes.Maze2
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-22 14:57:56
    public class Game : MonoBehaviour
    {
        [SerializeField] private TextMeshPro displayText;
        [SerializeField] private Agent[] agents;
        [SerializeField] private Player player;
        [SerializeField] private MazeVisualization visualization;
        [SerializeField] private int2 mazeSize = int2(20, 20);

        [SerializeField, Tooltip("Use zero for random seed.")]
        private int seed;

        [SerializeField, Range(0f, 1f)] private float pickLastProbability = 0.5f;
        [SerializeField, Range(0f, 1f)] private float openArbitraryProbability = 0.25f;
        [SerializeField, Range(0f, 1f)] private float openDeadEndProbability = 0.5f;

        private Maze _maze;
        private Scent _scent;
        private bool _isPlaying;
        private MazeCellObject[] _cellObjects;

        private void StartNewGame()
        {
            _isPlaying = true;
            displayText.gameObject.SetActive(false);
            _maze = new Maze(mazeSize);
            _scent = new Scent(_maze);
            new FindDiagonalPassagesJob
            {
                Maze = _maze
            }.ScheduleParallel(
                _maze.Length, _maze.SizeEW, new GenerateMazeJob
                {
                    Maze = _maze,
                    Seed = seed != 0 ? seed : Random.Range(1, int.MaxValue),
                    PickLastProbability = pickLastProbability,
                    OpenDeadEndProbability = openDeadEndProbability,
                    OpenArbitraryProbability = openArbitraryProbability
                }.Schedule()
            ).Complete();
            if (_cellObjects == null || _cellObjects.Length != _maze.Length)
            {
                _cellObjects = new MazeCellObject[_maze.Length];
            }

            visualization.Visualize(_maze, _cellObjects);
            if (seed != 0)
            {
                Random.InitState(seed);
            }

            player.StartNewGame(_maze.CoordinatesToWorldPosition(
                int2(Random.Range(0, mazeSize.x / 4), Random.Range(0, mazeSize.y / 4))
            ));
            var halfSize = mazeSize / 2;
            foreach (var agent in agents)
            {
                var coordinates =
                    int2(Random.Range(0, mazeSize.x), Random.Range(0, mazeSize.y));
                if (coordinates.x < halfSize.x && coordinates.y < halfSize.y)
                {
                    if (Random.value < 0.5f)
                    {
                        coordinates.x += halfSize.x;
                    }
                    else
                    {
                        coordinates.y += halfSize.y;
                    }
                }

                agent.StartNewGame(_maze, coordinates);
            }
        }

        private void Update()
        {
            if (_isPlaying)
            {
                UpdateGame();
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                StartNewGame();
                UpdateGame();
            }
        }

        private void UpdateGame()
        {
            var playerPosition = player.Move();
            var currentScent = _scent.Disperse(_maze, playerPosition);
            foreach (var agent in agents)
            {
                var agentPosition = agent.Move(currentScent);
                if (new Vector2(
                        agentPosition.x - playerPosition.x, agentPosition.z - playerPosition.z
                    ).sqrMagnitude < 1f)
                {
                    EndGame(agent.TriggerMessage);
                    return;
                }
            }
        }

        private void EndGame(string message)
        {
            _isPlaying = false;
            displayText.text = message;
            displayText.gameObject.SetActive(true);
            foreach (var agent in agents)
            {
                agent.EndGame();
            }

            foreach (var cellObj in _cellObjects)
            {
                cellObj.Recycle();
            }

            OnDestroy();
        }

        private void OnDestroy()
        {
            _maze.Dispose();
            _scent.Dispose();
        }
    }
}