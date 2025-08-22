using TMPro;
using Unity.Collections;
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
        [SerializeField, Range(0, 0.5f)] private float wallExtents = 0.25f;
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
        private MazeFlags _visibilityMask;

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

            UpdateOcclusion(playerPosition);
        }

        private void UpdateOcclusion(Vector3 playerPosition)
        {
            var isVisibleToPlayer = new NativeArray<bool>(agents.Length, Allocator.TempJob);
            var handle = new ClearOcclusionJob
            {
                Maze = _maze
            }.ScheduleParallel(_maze.Length, _maze.Length / 4, default);
            var wallExtentsInMazeSpace = _maze.WorldToMazeDistance(wallExtents);
            handle = new OcclusionJob
            {
                IsVisibleToPlayer = isVisibleToPlayer,
                Maze = _maze,
                Position = _maze.WorldToMazePosition(playerPosition),
                FieldOfView = player.Vision,
                WallExtents = wallExtentsInMazeSpace,
                VisibilityFlag = MazeFlags.VisibleToPlayer
            }.ScheduleParallel(4, 1, handle);

            for (var i = 0; i < agents.Length; i++)
            {
                var agent = agents[i];
                handle = new OcclusionJob
                {
                    IsVisibleToPlayer = isVisibleToPlayer,
                    IsVisibleToPlayerIndex = i,
                    Maze = _maze,
                    Position = _maze.WorldToMazePosition(agent.transform.localPosition),
                    FieldOfView = new FieldOfView
                    {
                        Range = _maze.WorldToMazeDistance(agent.LightRange),
                        Omnidirectional = true
                    },
                    WallExtents = wallExtentsInMazeSpace,
                    VisibilityFlag = (MazeFlags)((int)MazeFlags.VisbleToAgentA << i)
                }.ScheduleParallel(4, 1, handle);
            }

            handle.Complete();
            _visibilityMask = MazeFlags.VisibleToPlayer;
            for (var i = 0; i < agents.Length; i++)
            {
                var isVisible = isVisibleToPlayer[i];
                agents[i].SetLightEnabled(isVisible);
                if (isVisible)
                {
                    _visibilityMask |= (MazeFlags)((int)MazeFlags.VisbleToAgentA << i);
                }
            }

            isVisibleToPlayer.Dispose();
            for (var i = 0; i < _cellObjects.Length; i++)
            {
                _cellObjects[i].gameObject.SetActive(_maze[i].HasAny(_visibilityMask));
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

        private void OnDrawGizmosSelected()
        {
            if (!_isPlaying)
            {
                return;
            }

            var size = new Vector3(1.75f, 0.01f, 1.75f);
            for (var i = 0; i < _cellObjects.Length; i++)
            {
                var flags = _maze[i];
                if (flags.HasAny(MazeFlags.VisibleToAll))
                {
                    Gizmos.color = flags.Has(MazeFlags.VisibleToPlayer)
                        ? flags.HasAny(MazeFlags.VisibleToAllAgents)
                            ? Color.yellow
                            : Color.green
                        : flags.HasAny(_visibilityMask)
                            ? Color.red
                            : Color.blue;
                    var position = _cellObjects[i].transform.localPosition;
                    position.y = 0f;
                    Gizmos.DrawCube(position, size);
                }
            }
        }
    }
}