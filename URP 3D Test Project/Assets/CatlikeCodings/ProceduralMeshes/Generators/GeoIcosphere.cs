using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;
using quaternion = Unity.Mathematics.quaternion;

namespace CatlikeCodings.ProceduralMeshes.Generators
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-14 14:42:13
    public struct GeoIcosphere : IMeshGenerator
    {
        public int VertexCount => 5 * ResolutionV * Resolution + 2;
        public int IndexCount => 6 * 5 * ResolutionV * Resolution;
        public int JobLength => 5 * Resolution;
        public Bounds Bounds => new(Vector3.zero, new Vector3(2f, 2f, 2f));
        public int Resolution { get; set; }
        private int ResolutionV => 2 * Resolution;

        private struct Strip
        {
            public int Id;
            public float3 LowLeftCorner, LowRightCorner, HighLeftCorner, HighRightCorner;

            public float3 BottomLeftAxis,
                BottomRightAxis,
                MidLeftAxis,
                MidCenterAxis,
                MidRightAxis,
                TopLeftAxis,
                TopRightAxis;
        }

        private static float EdgeRotationAngle => acos(dot(up(), GetCorner(0, 1)));

        private static Strip GetStrip(int id) => id switch
        {
            0 => CreateStrip(0),
            1 => CreateStrip(1),
            2 => CreateStrip(2),
            3 => CreateStrip(3),
            _ => CreateStrip(4)
        };

        private static Strip CreateStrip(int id)
        {
            var s = new Strip
            {
                Id = id,
                LowLeftCorner = GetCorner(2 * id, -1),
                LowRightCorner = GetCorner(id == 4 ? 0 : 2 * id + 2, -1),
                HighLeftCorner = GetCorner(id == 0 ? 9 : 2 * id - 1, 1),
                HighRightCorner = GetCorner(2 * id + 1, 1)
            };
            s.BottomLeftAxis = normalize(cross(down(), s.LowLeftCorner));
            s.BottomRightAxis = normalize(cross(down(), s.LowRightCorner));
            s.MidLeftAxis = normalize(cross(s.LowLeftCorner, s.HighLeftCorner));
            s.MidCenterAxis = normalize(cross(s.LowLeftCorner, s.HighRightCorner));
            s.MidRightAxis = normalize(cross(s.LowRightCorner, s.HighRightCorner));
            s.TopLeftAxis = normalize(cross(s.HighLeftCorner, up()));
            s.TopRightAxis = normalize(cross(s.HighRightCorner, up()));
            return s;
        }

        private static float3 GetCorner(int id, int ySign) => float3(
            0.4f * sqrt(5f) * sin(0.2f * PI * id),
            ySign * 0.2f * sqrt(5f),
            -0.4f * sqrt(5f) * cos(0.2f * PI * id)
        );

        public void Execute<TS>(int i, TS streams) where TS : struct, IMeshStreams
        {
            var u = i / 5;
            var strip = GetStrip(i - 5 * u);
            var vi = ResolutionV * (Resolution * strip.Id + u) + 2;
            var ti = 2 * ResolutionV * (Resolution * strip.Id + u);
            var firstColumn = u == 0;
            var quad = int4(
                vi,
                firstColumn ? 0 : vi - ResolutionV,
                firstColumn
                    ? strip.Id == 0
                        ? 4 * ResolutionV * Resolution + 2
                        : vi - ResolutionV * (Resolution + u)
                    : vi - ResolutionV + 1,
                vi + 1
            );

            u += 1;
            var vertex = new Vertex();
            if (i == 0)
            {
                vertex.Position = down();
                streams.SetVertex(0, vertex);
                vertex.Position = up();
                streams.SetVertex(1, vertex);
            }

            vertex.Position = mul(
                quaternion.AxisAngle(
                    strip.BottomRightAxis, EdgeRotationAngle * u / Resolution
                ),
                down()
            );
            streams.SetVertex(vi, vertex);
            vi += 1;
            for (var v = 1; v < ResolutionV; v++, vi++, ti += 2)
            {
                float h = u + v;
                float3 leftAxis, rightAxis, leftStart, rightStart;
                float edgeAngleScale, faceAngleScale;
                if (v <= Resolution - u)
                {
                    leftAxis = strip.BottomLeftAxis;
                    rightAxis = strip.BottomRightAxis;
                    leftStart = rightStart = down();
                    edgeAngleScale = h / Resolution;
                    faceAngleScale = v / h;
                }
                else if (v < Resolution)
                {
                    leftAxis = strip.MidCenterAxis;
                    rightAxis = strip.MidRightAxis;
                    leftStart = strip.LowLeftCorner;
                    rightStart = strip.LowRightCorner;
                    edgeAngleScale = h / Resolution - 1f;
                    faceAngleScale = (Resolution - u) / (ResolutionV - h);
                }
                else if (v <= ResolutionV - u)
                {
                    leftAxis = strip.MidLeftAxis;
                    rightAxis = strip.MidCenterAxis;
                    leftStart = rightStart = strip.LowLeftCorner;
                    edgeAngleScale = h / Resolution - 1f;
                    faceAngleScale = (v - Resolution) / (h - Resolution);
                }
                else
                {
                    leftAxis = strip.TopLeftAxis;
                    rightAxis = strip.TopRightAxis;
                    leftStart = strip.HighLeftCorner;
                    rightStart = strip.HighRightCorner;
                    edgeAngleScale = h / Resolution - 2f;
                    faceAngleScale = (Resolution - u) / (3f * Resolution - h);
                }

                var pLeft = mul(
                    quaternion.AxisAngle(leftAxis, EdgeRotationAngle * edgeAngleScale),
                    leftStart
                );
                var pRight = mul(
                    quaternion.AxisAngle(rightAxis, EdgeRotationAngle * edgeAngleScale),
                    rightStart
                );
                var axis = normalize(cross(pRight, pLeft));
                var angle = acos(dot(pRight, pLeft)) * faceAngleScale;
                vertex.Position = mul(
                    quaternion.AxisAngle(axis, angle), pRight
                );
                streams.SetVertex(vi, vertex);
                streams.SetTriangle(ti + 0, quad.xyz);
                streams.SetTriangle(ti + 1, quad.xzw);
                quad.y = quad.z;
                quad += int4(1, 0, firstColumn && v <= Resolution - u ? ResolutionV : 1, 1);
            }

            if (!firstColumn)
            {
                quad.z = ResolutionV * Resolution * (strip.Id == 0 ? 5 : strip.Id) - Resolution + u + 1;
            }

            quad.w = u < Resolution ? quad.z + 1 : 1;

            streams.SetTriangle(ti + 0, quad.xyz);
            streams.SetTriangle(ti + 1, quad.xzw);
        }
    }
}