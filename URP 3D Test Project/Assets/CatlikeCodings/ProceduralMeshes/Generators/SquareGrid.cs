using UnityEngine;
using static Unity.Mathematics.math;

namespace CatlikeCodings.ProceduralMeshes.Generators
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-13 20:12:13
    public struct SquareGrid : IMeshGenerator
    {
        public int VertexCount => 4 * Resolution * Resolution;
        public int IndexCount => 6 * Resolution * Resolution;
        public int JobLength => Resolution;
        public Bounds Bounds => new(Vector3.zero, new Vector3(1f, 0f, 1f));
        public int Resolution { get; set; }

        public void Execute<TS>(int z, TS streams) where TS : struct, IMeshStreams
        {
            int vi = 4 * Resolution * z, ti = 2 * Resolution * z;
            for (var x = 0; x < Resolution; x++, vi += 4, ti += 2)
            {
                var xCoordinates = float2(x, x + 1f) / Resolution - 0.5f;
                var zCoordinates = float2(z, z + 1f) / Resolution - 0.5f;
                var vertex = new Vertex();
                vertex.Normal.y = 1f;
                vertex.Tangent.xw = float2(1f, -1f);
                vertex.Position.x = xCoordinates.x;
                vertex.Position.z = zCoordinates.x;
                streams.SetVertex(vi + 0, vertex);

                vertex.Position.x = xCoordinates.y;
                vertex.TexCoord0 = float2(1f, 0f);
                streams.SetVertex(vi + 1, vertex);

                vertex.Position.x = xCoordinates.x;
                vertex.Position.z = zCoordinates.y;
                vertex.TexCoord0 = float2(0f, 1f);
                streams.SetVertex(vi + 2, vertex);

                vertex.Position.x = xCoordinates.y;
                vertex.TexCoord0 = 1f;
                streams.SetVertex(vi + 3, vertex);

                streams.SetTriangle(ti + 0, vi + int3(0, 2, 1));
                streams.SetTriangle(ti + 1, vi + int3(1, 2, 3));
            }
        }
    }
}