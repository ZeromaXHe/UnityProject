using UnityEngine;

namespace CatlikeCodings.ProceduralMeshes
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-13 20:11:13
    public interface IMeshGenerator
    {
        void Execute<TS>(int i, TS streams) where TS : struct, IMeshStreams;
        int VertexCount { get; }
        int IndexCount { get; }
        int JobLength { get; }
        Bounds Bounds { get; }
        int Resolution { get; set; }
    }
}