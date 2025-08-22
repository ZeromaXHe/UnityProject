using Unity.Mathematics;

namespace CatlikeCodings.Prototypes.Maze2
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-22 21:32:22
    public struct FieldOfView
    {
        public float2 LeftLine, RightLine;
        public float Range;
        public bool Omnidirectional;
    }
}