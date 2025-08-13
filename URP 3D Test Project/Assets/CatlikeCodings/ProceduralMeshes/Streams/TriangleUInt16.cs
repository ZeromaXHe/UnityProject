using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace CatlikeCodings.ProceduralMeshes.Streams
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-13 20:34:13
    [StructLayout(LayoutKind.Sequential)]
    public struct TriangleUInt16 {
		
        public ushort a, b, c;

        public static implicit operator TriangleUInt16 (int3 t) => new()
        {
            a = (ushort)t.x,
            b = (ushort)t.y,
            c = (ushort)t.z
        };
    }
}