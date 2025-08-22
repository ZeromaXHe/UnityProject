using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace CatlikeCodings.Prototypes.Maze2
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-22 14:56:20
    public struct Maze
    {
        private readonly int2 _size;
        [NativeDisableParallelForRestriction] private NativeArray<MazeFlags> _cells;

        public int Length => _cells.Length;

        public Maze(int2 size)
        {
            _size = size;
            _cells = new NativeArray<MazeFlags>(size.x * size.y, Allocator.Persistent);
        }

        public void Dispose()
        {
            if (_cells.IsCreated)
            {
                _cells.Dispose();
            }
        }

        public MazeFlags this[int index]
        {
            get => _cells[index];
            set => _cells[index] = value;
        }

        public MazeFlags Set(int index, MazeFlags mask) =>
            _cells[index] = _cells[index].With(mask);

        public MazeFlags Unset(int index, MazeFlags mask) =>
            _cells[index] = _cells[index].Without(mask);

        public int SizeEW => _size.x;

        public int SizeNS => _size.y;

        public int StepN => _size.x;

        public int StepE => 1;

        public int StepS => -_size.x;

        public int StepW => -1;

        public int2 IndexToCoordinates(int index)
        {
            int2 coordinates;
            coordinates.y = index / _size.x;
            coordinates.x = index - _size.x * coordinates.y;
            return coordinates;
        }

        public Vector3 CoordinatesToWorldPosition(int2 coordinates, float y = 0f) =>
            new Vector3(
                2f * coordinates.x + 1f - _size.x,
                y,
                2f * coordinates.y + 1f - _size.y
            );

        public Vector3 IndexToWorldPosition(int index, float y = 0f) =>
            CoordinatesToWorldPosition(IndexToCoordinates(index), y);

        public int CoordinatesToIndex(int2 coordinates) =>
            coordinates.y * _size.x + coordinates.x;

        public int2 WorldPositionToCoordinates(Vector3 position) => int2(
            (int)((position.x + _size.x) * 0.5f),
            (int)((position.z + _size.y) * 0.5f)
        );

        public int WorldPositionToIndex(Vector3 position) =>
            CoordinatesToIndex(WorldPositionToCoordinates(position));

        public readonly float WorldToMazeDistance(float distance) => distance * 0.5f;

        public readonly float2 WorldToMazePosition(Vector3 position) =>
            (float2(position.x, position.z) + _size) * 0.5f;
    }
}