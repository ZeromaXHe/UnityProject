using Unity.Burst;
using Unity.Jobs;

namespace CatlikeCodings.Prototypes.Maze2
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-22 20:40:22
    [BurstCompile]
    public struct ClearOcclusionJob: IJobFor
    {
        public Maze Maze;

        public void Execute(int i) => Maze.Unset(i, MazeFlags.VisibleToAll);
    }
}