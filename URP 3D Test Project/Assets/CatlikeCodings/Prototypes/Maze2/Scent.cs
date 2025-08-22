using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace CatlikeCodings.Prototypes.Maze2
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-22 16:19:22
    public struct Scent
    {
        private NativeArray<float> _scentA, _scentB;
        private bool _useA;
        private float _cooldown;

        public Scent(Maze maze)
        {
            _scentA = new NativeArray<float>(
                maze.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory
            );
            _scentB = new NativeArray<float>(maze.Length, Allocator.Persistent);
            _useA = false;
            _cooldown = 0f;
        }

        public void Dispose()
        {
            if (_scentA.IsCreated)
            {
                _scentA.Dispose();
                _scentB.Dispose();
            }
        }

        public NativeArray<float> Disperse(Maze maze, Vector3 playerPosition)
        {
            _cooldown -= Time.deltaTime;
            if (_cooldown <= 0f)
            {
                _cooldown += 0.1f;
                new DisperseScentJob
                {
                    Maze = maze,
                    OldScent = _useA ? _scentA : _scentB,
                    NewScent = _useA ? _scentB : _scentA,
                }.ScheduleParallel(maze.Length, maze.SizeEW, default).Complete();

                _useA = !_useA;
            }

            var current = _useA ? _scentA : _scentB;
            current[maze.WorldPositionToIndex(playerPosition)] = 1f;
            return current;
        }
    }
}