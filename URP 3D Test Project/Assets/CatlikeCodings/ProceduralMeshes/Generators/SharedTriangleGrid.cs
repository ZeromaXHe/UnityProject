using UnityEngine;
using static Unity.Mathematics.math;

namespace CatlikeCodings.ProceduralMeshes.Generators
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-13 22:29:13
    public struct SharedTriangleGrid : IMeshGenerator
    {
        public int VertexCount => (Resolution + 1) * (Resolution + 1);
        public int IndexCount => 6 * Resolution * Resolution;
        public int JobLength => Resolution + 1;
        public Bounds Bounds => new(Vector3.zero, new Vector3(1f + 0.5f / Resolution, 0f, sqrt(3f) / 2f));
        public int Resolution { get; set; }

        public void Execute<TS>(int z, TS streams) where TS : struct, IMeshStreams
        {
            int vi = (Resolution + 1) * z, ti = 2 * Resolution * (z - 1);
            var xOffset = -0.25f;
            var uOffset = 0f;
            int iA = -Resolution - 2, iB = -Resolution - 1, iC = -1, iD = 0;
            var tA = int3(iA, iC, iD);
            var tB = int3(iA, iD, iB);
            if ((z & 1) == 1)
            {
                xOffset = 0.25f;
                uOffset = 0.5f / (Resolution + 0.5f);
                tA = int3(iA, iC, iB);
                tB = int3(iB, iC, iD);
            }

            xOffset = xOffset / Resolution - 0.5f;

            var vertex = new Vertex();
            vertex.Normal.y = 1f;
            vertex.Tangent.xw = float2(1f, -1f);

            vertex.Position.x = xOffset;
            vertex.Position.z = ((float)z / Resolution - 0.5f) * sqrt(3f) / 2f;
            vertex.TexCoord0.x = uOffset;
            vertex.TexCoord0.y = vertex.Position.z / (1f + 0.5f / Resolution) + 0.5f;
            streams.SetVertex(vi, vertex);
            vi += 1;

            for (var x = 1; x <= Resolution; x++, vi++, ti += 2)
            {
                vertex.Position.x = (float)x / Resolution + xOffset;
                vertex.TexCoord0.x = x / (Resolution + 0.5f) + uOffset;
                streams.SetVertex(vi, vertex);
                if (z > 0)
                {
                    streams.SetTriangle(ti + 0, vi + tA);
                    streams.SetTriangle(ti + 1, vi + tB);
                }
            }
        }
    }
}