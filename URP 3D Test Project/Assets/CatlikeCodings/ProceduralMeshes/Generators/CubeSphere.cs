using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace CatlikeCodings.ProceduralMeshes.Generators
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-14 09:54:13
    public struct CubeSphere : IMeshGenerator
    {
        public int VertexCount => 6 * 4 * Resolution * Resolution;
        public int IndexCount => 6 * 6 * Resolution * Resolution;
        public int JobLength => 6 * Resolution;
        public Bounds Bounds => new(Vector3.zero, new Vector3(2f, 2f, 2f));
        public int Resolution { get; set; }

        private struct Side
        {
            public int Id;
            public float3 UvOrigin, UVector, VVector;
        }

        private static Side GetSide(int id) =>
            id switch
            {
                0 => new Side
                {
                    Id = id,
                    UvOrigin = -1f,
                    UVector = 2f * right(),
                    VVector = 2f * up()
                },
                1 => new Side
                {
                    Id = id,
                    UvOrigin = float3(1f, -1f, -1f),
                    UVector = 2f * forward(),
                    VVector = 2f * up()
                },
                2 => new Side
                {
                    Id = id,
                    UvOrigin = -1f,
                    UVector = 2f * forward(),
                    VVector = 2f * right()
                },
                3 => new Side
                {
                    Id = id,
                    UvOrigin = float3(-1f, -1f, 1f),
                    UVector = 2f * up(),
                    VVector = 2f * right()
                },
                4 => new Side
                {
                    Id = id,
                    UvOrigin = -1f,
                    UVector = 2f * up(),
                    VVector = 2f * forward()
                },
                _ => new Side
                {
                    Id = id,
                    UvOrigin = float3(-1f, 1f, -1f),
                    UVector = 2f * right(),
                    VVector = 2f * forward()
                }
            };

        private static float3 CubeToSphere(float3 p) => p * sqrt(
            1f - ((p * p).yxx + (p * p).zzy) / 2f + (p * p).yxx * (p * p).zzy / 3f
        );

        public void Execute<TS>(int i, TS streams) where TS : struct, IMeshStreams
        {
            var u = i / 6;
            var side = GetSide(i - 6 * u);
            var vi = 4 * Resolution * (Resolution * side.Id + u);
            var ti = 2 * Resolution * (Resolution * side.Id + u);
            var uA = side.UvOrigin + side.UVector * u / Resolution;
            var uB = side.UvOrigin + side.UVector * (u + 1) / Resolution;
            float3 pA = CubeToSphere(uA), pB = CubeToSphere(uB);
            var vertex = new Vertex();
            vertex.Tangent = float4(normalize(pB - pA), -1f);
            for (var v = 1; v <= Resolution; v++, vi += 4, ti += 2)
            {
                var pC = CubeToSphere(uA + side.VVector * v / Resolution);
                var pD = CubeToSphere(uB + side.VVector * v / Resolution);

                vertex.Position = pA;
                vertex.Normal = normalize(cross(pC - pA, vertex.Tangent.xyz));
                vertex.TexCoord0 = 0f;
                streams.SetVertex(vi + 0, vertex);

                vertex.Position = pB;
                vertex.Normal = normalize(cross(pD - pB, vertex.Tangent.xyz));
                vertex.TexCoord0 = float2(1f, 0f);
                streams.SetVertex(vi + 1, vertex);

                vertex.Position = pC;
                vertex.Tangent.xyz = normalize(pD - pC);
                vertex.Normal = normalize(cross(pC - pA, vertex.Tangent.xyz));
                vertex.TexCoord0 = float2(0f, 1f);
                streams.SetVertex(vi + 2, vertex);

                vertex.Position = pD;
                vertex.Normal = normalize(cross(pD - pB, vertex.Tangent.xyz));
                vertex.TexCoord0 = 1f;
                streams.SetVertex(vi + 3, vertex);

                streams.SetTriangle(ti + 0, vi + int3(0, 2, 1));
                streams.SetTriangle(ti + 1, vi + int3(1, 2, 3));
                pA = pC;
                pB = pD;
            }
        }
    }
}