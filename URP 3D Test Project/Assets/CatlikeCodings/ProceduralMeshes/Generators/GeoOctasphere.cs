using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;
using quaternion = Unity.Mathematics.quaternion;

namespace CatlikeCodings.ProceduralMeshes.Generators
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-14 11:47:13
    public struct GeoOctasphere : IMeshGenerator
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
            var vertex = new Vertex();
            sincos(PI + PI * u / (2 * Resolution), out var sine, out vertex.Position.y);
            vertex.Position -= sine * rhombus.RightCorner;
            vertex.Normal = vertex.Position;
            vertex.Tangent.xz = GetTangentXZ(vertex.Position);
            vertex.Tangent.w = -1f;
            vertex.TexCoord0.x = rhombus.Id * 0.25f + 0.25f;
            vertex.TexCoord0.y = (float)u / (2 * Resolution);
            streams.SetVertex(vi, vertex);
            vi += 1;
            for (var v = 1; v < Resolution; v++, vi++, ti += 2)
            {
                float h = u + v;
                float3 pRight = 0f;
                sincos(PI + PI * h / (2 * Resolution), out sine, out pRight.y);
                var pLeft = pRight - sine * rhombus.LeftCorner;
                pRight -= sine * rhombus.RightCorner;
                var axis = normalize(cross(pRight, pLeft));
                var angle = acos(dot(pRight, pLeft)) * (
                    v <= Resolution - u ? v / h : (Resolution - u) / (2f * Resolution - h)
                );
                vertex.Normal = vertex.Position = mul(
                    quaternion.AxisAngle(axis, angle), pRight
                );
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
                sincos(
                    PI + PI * v / (2 * Resolution),
                    out vertex.Position.z, out vertex.Position.y
                );

                vertex.Normal = vertex.Position;
                vertex.TexCoord0.y = (float)v / (2 * Resolution);
                streams.SetVertex(v + 7, vertex);
            }
        }
    }
}