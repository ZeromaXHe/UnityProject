using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace CatlikeCodings.ProceduralMeshes.Generators
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-14 10:23:13
    public struct SharedCubeSphere : IMeshGenerator
    {
        public int VertexCount => 6 * Resolution * Resolution + 2;
        public int IndexCount => 6 * 6 * Resolution * Resolution;
        public int JobLength => 6 * Resolution;
        public Bounds Bounds => new(Vector3.zero, new Vector3(2f, 2f, 2f));
        public int Resolution { get; set; }

        private struct Side
        {
            public int Id;
            public float3 UvOrigin, UVector, VVector;
            public int SeamStep;
            public bool TouchesMinimumPole => (Id & 1) == 0;
        }

        private static Side GetSide(int id) =>
            id switch
            {
                0 => new Side
                {
                    Id = id,
                    UvOrigin = -1f,
                    UVector = 2f * right(),
                    VVector = 2f * up(),
                    SeamStep = 4
                },
                1 => new Side
                {
                    Id = id,
                    UvOrigin = float3(1f, -1f, -1f),
                    UVector = 2f * forward(),
                    VVector = 2f * up(),
                    SeamStep = 4
                },
                2 => new Side
                {
                    Id = id,
                    UvOrigin = -1f,
                    UVector = 2f * forward(),
                    VVector = 2f * right(),
                    SeamStep = -2
                },
                3 => new Side
                {
                    Id = id,
                    UvOrigin = float3(-1f, -1f, 1f),
                    UVector = 2f * up(),
                    VVector = 2f * right(),
                    SeamStep = -2
                },
                4 => new Side
                {
                    Id = id,
                    UvOrigin = -1f,
                    UVector = 2f * up(),
                    VVector = 2f * forward(),
                    SeamStep = -2
                },
                _ => new Side
                {
                    Id = id,
                    UvOrigin = float3(-1f, 1f, -1f),
                    UVector = 2f * right(),
                    VVector = 2f * forward(),
                    SeamStep = -2
                }
            };

        private static float3 CubeToSphere(float3 p) => p * sqrt(
            1f - ((p * p).yxx + (p * p).zzy) / 2f + (p * p).yxx * (p * p).zzy / 3f
        );

        public void Execute<TS>(int i, TS streams) where TS : struct, IMeshStreams
        {
            var u = i / 6;
            var side = GetSide(i - 6 * u);
            var vi = Resolution * (Resolution * side.Id + u) + 2;
            var ti = 2 * Resolution * (Resolution * side.Id + u);
            var firstColumn = u == 0;
            u += 1;
            var pStart = side.UvOrigin + side.UVector * u / Resolution;
            var vertex = new Vertex();
            if (i == 0)
            {
                vertex.Position = -sqrt(1f / 3f);
                streams.SetVertex(0, vertex);
                vertex.Position = sqrt(1f / 3f);
                streams.SetVertex(1, vertex);
            }

            vertex.Position = CubeToSphere(pStart);
            streams.SetVertex(vi, vertex);
            var triangle = int3(
                vi,
                firstColumn && side.TouchesMinimumPole ? 0 : vi - Resolution,
                vi + (firstColumn
                    ? side.TouchesMinimumPole ? side.SeamStep * Resolution * Resolution :
                    Resolution == 1 ? side.SeamStep : -Resolution + 1
                    : -Resolution + 1
                )
            );
            streams.SetTriangle(ti, triangle);
            vi += 1;
            ti += 1;
            var zAdd = firstColumn && side.TouchesMinimumPole ? Resolution : 1;
            var zAddLast = firstColumn && side.TouchesMinimumPole
                ? Resolution
                : !firstColumn && !side.TouchesMinimumPole
                    ? Resolution * ((side.SeamStep + 1) * Resolution - u) + u
                    : (side.SeamStep + 1) * Resolution * Resolution - Resolution + 1;
            for (var v = 1; v < Resolution; v++, vi++, ti += 2)
            {
                vertex.Position = CubeToSphere(pStart + side.VVector * v / Resolution);
                streams.SetVertex(vi, vertex);
                triangle.x += 1;
                triangle.y = triangle.z;
                triangle.z += v == Resolution - 1 ? zAddLast : zAdd;

                streams.SetTriangle(ti + 0, int3(triangle.x - 1, triangle.y, triangle.x));
                streams.SetTriangle(ti + 1, triangle);
            }

            streams.SetTriangle(ti, int3(
                triangle.x,
                triangle.z,
                side.TouchesMinimumPole ? triangle.z + Resolution : u == Resolution ? 1 : triangle.z + 1
            ));
        }
    }
}