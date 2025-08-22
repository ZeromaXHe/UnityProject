using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace CatlikeCodings.Prototypes.Maze2
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-22 15:33:22
    [BurstCompile]
    public struct FindDiagonalPassagesJob: IJobFor
    {
        public Maze Maze;

        public void Execute (int i)
        {
            var cell = Maze[i];
            if (
                cell.Has(MazeFlags.PassageN | MazeFlags.PassageE) &&
                Maze[i + Maze.StepN + Maze.StepE].Has(MazeFlags.PassageS | MazeFlags.PassageW)
            )
            {
                cell = cell.With(MazeFlags.PassageNE);
            }
            if (
                cell.Has(MazeFlags.PassageN | MazeFlags.PassageW) &&
                Maze[i + Maze.StepN + Maze.StepW].Has(MazeFlags.PassageS | MazeFlags.PassageE)
            )
            {
                cell = cell.With(MazeFlags.PassageNW);
            }
            if (
                cell.Has(MazeFlags.PassageS | MazeFlags.PassageE) &&
                Maze[i + Maze.StepS + Maze.StepE].Has(MazeFlags.PassageN | MazeFlags.PassageW)
            )
            {
                cell = cell.With(MazeFlags.PassageSE);
            }
            if (
                cell.Has(MazeFlags.PassageS | MazeFlags.PassageW) &&
                Maze[i + Maze.StepS + Maze.StepW].Has(MazeFlags.PassageN | MazeFlags.PassageE)
            )
            {
                cell = cell.With(MazeFlags.PassageSW);
            }
            Maze[i] = cell;
        }
    }
}