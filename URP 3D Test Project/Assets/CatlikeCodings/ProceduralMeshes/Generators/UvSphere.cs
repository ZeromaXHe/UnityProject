using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace CatlikeCodings.ProceduralMeshes.Generators
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-13 23:11:13
    public struct UvSphere : IMeshGenerator
    {
        public int VertexCount => (ResolutionU + 1) * (ResolutionV + 1) - 2;
        public int IndexCount => 6 * ResolutionU * (ResolutionV - 1);
        public int JobLength => ResolutionU + 1;
        public Bounds Bounds => new(Vector3.zero, new Vector3(2f, 2f, 2f));
        public int Resolution { get; set; }
        private int ResolutionU => 4 * Resolution;
        private int ResolutionV => 2 * Resolution;

        public void Execute<TS>(int u, TS streams) where TS : struct, IMeshStreams
        {
            if (u == 0)
            {
                ExecuteSeam(streams);
            }
            else
            {
                ExecuteRegular(u, streams);
            }
        }

        private void ExecuteRegular<TS>(int u, TS streams) where TS : struct, IMeshStreams
        {
            int vi = (ResolutionV + 1) * u - 2, ti = 2 * (ResolutionV - 1) * (u - 1);
            var vertex = new Vertex();
            vertex.Position.y = vertex.Normal.y = -1f;
            sincos(2f * PI * (u - 0.5f) / ResolutionU,
                out vertex.Tangent.z, out vertex.Tangent.x);
            vertex.Tangent.w = -1f;
            vertex.TexCoord0.x = (u - 0.5f) / ResolutionU;
            streams.SetVertex(vi, vertex);

            vertex.Position.y = vertex.Normal.y = 1f;
            vertex.TexCoord0.y = 1f;
            streams.SetVertex(vi + ResolutionV, vertex);
            vi += 1;

            float2 circle;
            sincos(2f * PI * u / ResolutionU, out circle.x, out circle.y);
            vertex.Tangent.xz = circle.yx;
            circle.y = -circle.y;
            vertex.TexCoord0.x = (float)u / ResolutionU;

            var shiftLeft = (u == 1 ? 0 : -1) - ResolutionV;
            streams.SetTriangle(ti, vi + int3(-1, shiftLeft, 0));
            ti += 1;

            for (var v = 1; v < ResolutionV; v++, vi++)
            {
                sincos(
                    PI + PI * v / ResolutionV,
                    out var circleRadius, out vertex.Position.y
                );
                vertex.Position.xz = circle * -circleRadius;
                vertex.Normal = vertex.Position;
                vertex.TexCoord0.y = (float)v / ResolutionV;
                streams.SetVertex(vi, vertex);
                if (v > 1)
                {
                    streams.SetTriangle(ti + 0, vi + int3(shiftLeft - 1, shiftLeft, -1));
                    streams.SetTriangle(ti + 1, vi + int3(-1, shiftLeft, 0));
                    ti += 2;
                }
            }

            streams.SetTriangle(ti, vi + int3(shiftLeft - 1, 0, -1));
        }

        private void ExecuteSeam<TS>(TS streams) where TS : struct, IMeshStreams
        {
            var vertex = new Vertex();
            vertex.Tangent.x = 1f;
            vertex.Tangent.w = -1f;
            for (var v = 1; v < ResolutionV; v++)
            {
                sincos(
                    PI + PI * v / ResolutionV,
                    out vertex.Position.z, out vertex.Position.y
                );
                vertex.Normal = vertex.Position;
                vertex.TexCoord0.y = (float)v / ResolutionV;
                streams.SetVertex(v - 1, vertex);
            }
        }
    }
}