using UnityEngine;
using static Unity.Mathematics.math;

namespace CatlikeCodings.ProceduralMeshes.Generators
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-13 21:18:13
    public struct SharedSquareGrid : IMeshGenerator
    {
        public int VertexCount => (Resolution + 1) * (Resolution + 1);
        public int IndexCount => 6 * Resolution * Resolution;
        public int JobLength => Resolution + 1;
        public Bounds Bounds => new(Vector3.zero, new Vector3(1f, 0f, 1f));
        public int Resolution { get; set; }

        public void Execute<TS>(int z, TS streams) where TS : struct, IMeshStreams
        {
            int vi = (Resolution + 1) * z, ti = 2 * Resolution * (z - 1);
            var vertex = new Vertex();
            vertex.Normal.y = 1f;
            vertex.Tangent.xw = float2(1f, -1f);

            vertex.Position.x = -0.5f;
            vertex.Position.z = (float)z / Resolution - 0.5f;
            vertex.TexCoord0.y = (float)z / Resolution;
            streams.SetVertex(vi, vertex);
            vi += 1;

            for (var x = 1; x <= Resolution; x++, vi++, ti += 2)
            {
                vertex.Position.x = (float)x / Resolution - 0.5f;
                vertex.TexCoord0.x = (float)x / Resolution;
                streams.SetVertex(vi, vertex);
                if (z > 0)
                {
                    streams.SetTriangle(ti + 0, vi + int3(-Resolution - 2, -1, -Resolution - 1));
                    streams.SetTriangle(ti + 1, vi + int3(-Resolution - 1, -1, 0));
                }
            }
        }
    }
}