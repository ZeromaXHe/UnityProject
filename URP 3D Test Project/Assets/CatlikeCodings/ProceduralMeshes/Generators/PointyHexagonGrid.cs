using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace CatlikeCodings.ProceduralMeshes.Generators
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-13 22:38:13
    public struct PointyHexagonGrid : IMeshGenerator
    {
        public int VertexCount => 7 * Resolution * Resolution;
        public int IndexCount => 18 * Resolution * Resolution;
        public int JobLength => Resolution;

        public Bounds Bounds => new(Vector3.zero, new Vector3(
            (Resolution > 1 ? 0.5f + 0.25f / Resolution : 0.5f) * sqrt(3f),
            0f,
            0.75f + 0.25f / Resolution
        ));

        public int Resolution { get; set; }

        public void Execute<TS>(int z, TS streams) where TS : struct, IMeshStreams
        {
            int vi = 7 * Resolution * z, ti = 6 * Resolution * z;
            var h = sqrt(3f) / 4f;
            float2 centerOffset = 0f;
            if (Resolution > 1)
            {
                centerOffset.x = (((z & 1) == 0 ? 0.5f : 1.5f) - Resolution) * h;
                centerOffset.y = -0.375f * (Resolution - 1);
            }

            for (var x = 0; x < Resolution; x++, vi += 7, ti += 6)
            {
                var center = (float2(2f * h * x, 0.75f * z) + centerOffset) / Resolution;
                var xCoordinates = center.x + float2(-h, h) / Resolution;
                var zCoordinates = center.y + float4(-0.5f, -0.25f, 0.25f, 0.5f) / Resolution;
                var vertex = new Vertex();
                vertex.Normal.y = 1f;
                vertex.Tangent.xw = float2(1f, -1f);
                vertex.Position.xz = center;
                vertex.TexCoord0 = 0.5f;
                streams.SetVertex(vi + 0, vertex);

                vertex.Position.z = zCoordinates.x;
                vertex.TexCoord0.y = 0f;
                streams.SetVertex(vi + 1, vertex);

                vertex.Position.x = xCoordinates.x;
                vertex.Position.z = zCoordinates.y;
                vertex.TexCoord0 = float2(0.5f - h, 0.25f);
                streams.SetVertex(vi + 2, vertex);

                vertex.Position.z = zCoordinates.z;
                vertex.TexCoord0.y = 0.75f;
                streams.SetVertex(vi + 3, vertex);

                vertex.Position.x = center.x;
                vertex.Position.z = zCoordinates.w;
                vertex.TexCoord0 = float2(0.5f, 1f);
                streams.SetVertex(vi + 4, vertex);

                vertex.Position.x = xCoordinates.y;
                vertex.Position.z = zCoordinates.z;
                vertex.TexCoord0 = float2(0.5f + h, 0.75f);
                streams.SetVertex(vi + 5, vertex);

                vertex.Position.z = zCoordinates.y;
                vertex.TexCoord0.y = 0.25f;
                streams.SetVertex(vi + 6, vertex);

                streams.SetTriangle(ti + 0, vi + int3(0, 1, 2));
                streams.SetTriangle(ti + 1, vi + int3(0, 2, 3));
                streams.SetTriangle(ti + 2, vi + int3(0, 3, 4));
                streams.SetTriangle(ti + 3, vi + int3(0, 4, 5));
                streams.SetTriangle(ti + 4, vi + int3(0, 5, 6));
                streams.SetTriangle(ti + 5, vi + int3(0, 6, 1));
            }
        }
    }
}