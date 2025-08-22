using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace CatlikeCodings.Prototypes.Maze2
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-22 16:17:22
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
    public struct DisperseScentJob: IJobFor
    {
        [ReadOnly]
        public Maze Maze;

        [ReadOnly, NativeDisableParallelForRestriction]
        public NativeArray<float> OldScent;

        public NativeArray<float> NewScent;

        public void Execute(int i)
        {
            var cell = Maze[i];
            var scent = OldScent[i];

            var fromNeighbors = 0f;
            var dispersalFactor = 0f;
            if (cell.Has(MazeFlags.PassageE))
            {
                fromNeighbors += OldScent[i + Maze.StepE];
                dispersalFactor += 1f;
            }
            if (cell.Has(MazeFlags.PassageW))
            {
                fromNeighbors += OldScent[i + Maze.StepW];
                dispersalFactor += 1f;
            }
            if (cell.Has(MazeFlags.PassageN))
            {
                fromNeighbors += OldScent[i + Maze.StepN];
                dispersalFactor += 1f;
            }
            if (cell.Has(MazeFlags.PassageS))
            {
                fromNeighbors += OldScent[i + Maze.StepS];
                dispersalFactor += 1f;
            }

            scent += (fromNeighbors - scent * dispersalFactor) * 0.2f;
            NewScent[i] = scent * 0.5f;
        }
    }
}