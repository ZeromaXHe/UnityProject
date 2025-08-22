using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace CatlikeCodings.Prototypes.Maze2
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-22 20:43:52
    public partial struct OcclusionJob
    {
        struct Scan
        {
            public int Index;
            public float2 Offset;
            public float LeftSlope, RightSlope;

            public void ShiftEast(int index, float xOffset, float leftSlope)
            {
                Index = index;
                Offset.x = xOffset;
                LeftSlope = leftSlope;
            }

            public readonly Scan ShiftedNorth(int indexOffset, float rightSlope) => new()
            {
                Index = Index + indexOffset,
                Offset = float2(Offset.x, Offset.y + 1f),
                LeftSlope = LeftSlope,
                RightSlope = rightSlope
            };
        }
    }
}