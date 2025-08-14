using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace CatlikeCodings.ProceduralMeshes.Generators
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-14 10:59:13
    public struct Octasphere : IMeshGenerator
    {
        public int VertexCount => 4 * Resolution * Resolution + 2 * Resolution + 7;
        public int IndexCount => 6 * 4 * Resolution * Resolution;
        public int JobLength => 4 * Resolution + 1;
        public Bounds Bounds => new(Vector3.zero, new Vector3(2f, 2f, 2f));
        public int Resolution { get; set; }

        private struct Rhombus
        {
            public int Id;
            public float3 LeftCorner, RightCorner;
        }

        private static Rhombus GetRhombus(int id) =>
            id switch
            {
                0 => new Rhombus
                {
                    Id = id,
                    LeftCorner = back(),
                    RightCorner = right()
                },
                1 => new Rhombus
                {
                    Id = id,
                    LeftCorner = right(),
                    RightCorner = forward()
                },
                2 => new Rhombus
                {
                    Id = id,
                    LeftCorner = forward(),
                    RightCorner = left()
                },
                _ => new Rhombus
                {
                    Id = id,
                    LeftCorner = left(),
                    RightCorner = back()
                }
            };

        private static float2 GetTangentXZ(float3 p) => normalize(float2(-p.z, p.x));

        private static float2 GetTexCoord(float3 p)
        {
            var texCoord = float2(
                atan2(p.x, p.z) / (-2f * PI) + 0.5f,
                asin(p.y) / PI + 0.5f
            );
            if (texCoord.x < 1e-6f)
            {
                texCoord.x = 1f;
            }

            return texCoord;
        }

        public void Execute<TS>(int i, TS streams) where TS : struct, IMeshStreams
        {
            if (i == 0)
            {
                ExecutePolesAndSeam(streams);
            }
            else
            {
                ExecuteRegular(i - 1, streams);
            }
        }

        private void ExecuteRegular<TS>(int i, TS streams) where TS : struct, IMeshStreams
        {
            var u = i / 4;
            var rhombus = GetRhombus(i - 4 * u);
            var vi = Resolution * (Resolution * rhombus.Id + u + 2) + 7;
            var ti = 2 * Resolution * (Resolution * rhombus.Id + u);
            var firstColumn = u == 0;
            var quad = int4(
                vi,
                firstColumn ? rhombus.Id : vi - Resolution,
                firstColumn ? rhombus.Id == 0 ? 8 : vi - Resolution * (Resolution + u) : vi - Resolution + 1,
                vi + 1
            );

            u += 1;
            var columnBottomDir = rhombus.RightCorner - down();
            var columnBottomStart = down() + columnBottomDir * u / Resolution;
            var columnBottomEnd = rhombus.LeftCorner + columnBottomDir * u / Resolution;
            var columnTopDir = up() - rhombus.LeftCorner;
            var columnTopStart = rhombus.RightCorner + columnTopDir * ((float)u / Resolution - 1f);
            var columnTopEnd = rhombus.LeftCorner + columnTopDir * u / Resolution;
            var vertex = new Vertex();
            vertex.Normal = vertex.Position = normalize(columnBottomStart);
            vertex.Tangent.xz = GetTangentXZ(vertex.Position);
            vertex.Tangent.w = -1f;
            vertex.TexCoord0 = GetTexCoord(vertex.Position);
            streams.SetVertex(vi, vertex);
            vi += 1;
            for (var v = 1; v < Resolution; v++, vi++, ti += 2)
            {
                if (v <= Resolution - u)
                {
                    vertex.Position = lerp(columnBottomStart, columnBottomEnd, (float)v / Resolution);
                }
                else
                {
                    vertex.Position = lerp(columnTopStart, columnTopEnd, (float)v / Resolution);
                }

                vertex.Normal = vertex.Position = normalize(vertex.Position);
                vertex.Tangent.xz = GetTangentXZ(vertex.Position);
                vertex.TexCoord0 = GetTexCoord(vertex.Position);
                streams.SetVertex(vi, vertex);
                streams.SetTriangle(ti + 0, quad.xyz);
                streams.SetTriangle(ti + 1, quad.xzw);
                quad.y = quad.z;
                quad += int4(1, 0, firstColumn && rhombus.Id != 0 ? Resolution : 1, 1);
            }

            quad.z = Resolution * Resolution * rhombus.Id + Resolution + u + 6;
            quad.w = u < Resolution ? quad.z + 1 : rhombus.Id + 4;

            streams.SetTriangle(ti + 0, quad.xyz);
            streams.SetTriangle(ti + 1, quad.xzw);
        }

        private void ExecutePolesAndSeam<TS>(TS streams) where TS : struct, IMeshStreams
        {
            var vertex = new Vertex();
            vertex.Tangent = float4(sqrt(0.5f), 0f, sqrt(0.5f), -1f);
            vertex.TexCoord0.x = 0.125f;
            for (var i = 0; i < 4; i++)
            {
                vertex.Position = vertex.Normal = down();
                vertex.TexCoord0.y = 0f;
                streams.SetVertex(i, vertex);
                vertex.Position = vertex.Normal = up();
                vertex.TexCoord0.y = 1f;
                streams.SetVertex(i + 4, vertex);
                vertex.Tangent.xz = float2(-vertex.Tangent.z, vertex.Tangent.x);
                vertex.TexCoord0.x += 0.25f;
            }

            vertex.Tangent.xz = float2(1f, 0f);
            vertex.TexCoord0.x = 0f;

            for (var v = 1; v < 2 * Resolution; v++)
            {
                if (v < Resolution)
                {
                    vertex.Position = lerp(down(), back(), (float)v / Resolution);
                }
                else
                {
                    vertex.Position =
                        lerp(back(), up(), (float)(v - Resolution) / Resolution);
                }

                vertex.Normal = vertex.Position = normalize(vertex.Position);
                vertex.TexCoord0.y = GetTexCoord(vertex.Position).y;
                streams.SetVertex(v + 7, vertex);
            }
        }
    }
}