using CatlikeCodings.ProceduralMeshes.Streams;
using CatlikeCodings.PseudorandomNoise.Hashing;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;
using static CatlikeCodings.PseudorandomNoise.Hashing.Noise;

namespace CatlikeCodings.PseudorandomSurfaces
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-14 23:06:14
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct SurfaceJob<TN> : IJobFor where TN : struct, INoise
    {
        private struct Vertex4
        {
            public SingleStream.Stream0 V0, V1, V2, V3;
        }

        private NativeArray<Vertex4> _vertices;
        private Settings _settings;
        private float3x4 _domainTRS;
        private float3x3 _derivativeMatrix;
        private float _displacement;
        private bool _isPlane;

        public void Execute(int i)
        {
            var v = _vertices[i];
            var noise = GetFractalNoise<TN>(
                _domainTRS.TransformVectors(transpose(
                    float3x4(v.V0.position, v.V1.position, v.V2.position, v.V3.position))),
                _settings) * _displacement;
            noise.Derivatives = _derivativeMatrix.TransformVectors(noise.Derivatives);
            if (_isPlane)
            {
                _vertices[i] = SetPlaneVertices(v, noise);
            }
            else
            {
                _vertices[i] = SetSphereVertices(v, noise);
            }
        }

        private static Vertex4 SetPlaneVertices(Vertex4 v, Sample4 noise)
        {
            v.V0.position.y = noise.V.x;
            v.V1.position.y = noise.V.y;
            v.V2.position.y = noise.V.z;
            v.V3.position.y = noise.V.w;
            var normalizer = rsqrt(noise.Dx * noise.Dx + 1f);
            var tangentY = noise.Dx * normalizer;
            v.V0.tangent = float4(normalizer.x, tangentY.x, 0f, -1f);
            v.V1.tangent = float4(normalizer.y, tangentY.y, 0f, -1f);
            v.V2.tangent = float4(normalizer.z, tangentY.z, 0f, -1f);
            v.V3.tangent = float4(normalizer.w, tangentY.w, 0f, -1f);
            normalizer = rsqrt(noise.Dx * noise.Dx + noise.Dz * noise.Dz + 1f);
            var normalX = -noise.Dx * normalizer;
            var normalZ = -noise.Dz * normalizer;
            v.V0.normal = float3(normalX.x, normalizer.x, normalZ.x);
            v.V1.normal = float3(normalX.y, normalizer.y, normalZ.y);
            v.V2.normal = float3(normalX.z, normalizer.z, normalZ.z);
            v.V3.normal = float3(normalX.w, normalizer.w, normalZ.w);
            return v;
        }

        private static Vertex4 SetSphereVertices(Vertex4 v, Sample4 noise)
        {
            noise.V += 1f;
            noise.Dx /= noise.V;
            noise.Dy /= noise.V;
            noise.Dz /= noise.V;
            var p = transpose(float3x4(v.V0.position, v.V1.position, v.V2.position, v.V3.position));
            var tangentCheck = abs(v.V0.tangent.xyz);
            if (tangentCheck.x + tangentCheck.y + tangentCheck.z > 0f)
            {
                var t = transpose(float3x4(v.V0.tangent.xyz, v.V1.tangent.xyz, v.V2.tangent.xyz, v.V3.tangent.xyz));
                var td = t.c0 * noise.Dx + t.c1 * noise.Dy + t.c2 * noise.Dz;
                t.c0 += td * p.c0;
                t.c1 += td * p.c1;
                t.c2 += td * p.c2;
                var tt = transpose(t.NormalizeRows());
                v.V0.tangent = float4(tt.c0, -1f);
                v.V1.tangent = float4(tt.c1, -1f);
                v.V2.tangent = float4(tt.c2, -1f);
                v.V3.tangent = float4(tt.c3, -1f);
            }

            var pd = p.c0 * noise.Dx + p.c1 * noise.Dy + p.c2 * noise.Dz;
            var nt = transpose(float4x3(
                p.c0 - noise.Dx + pd * p.c0,
                p.c1 - noise.Dy + pd * p.c1,
                p.c2 - noise.Dz + pd * p.c2
            ).NormalizeRows());
            v.V0.normal = nt.c0;
            v.V1.normal = nt.c1;
            v.V2.normal = nt.c2;
            v.V3.normal = nt.c3;
            v.V0.position *= noise.V.x;
            v.V1.position *= noise.V.y;
            v.V2.position *= noise.V.z;
            v.V3.position *= noise.V.w;
            return v;
        }

        public static JobHandle ScheduleParallel(Mesh.MeshData meshData, int resolution,
            Settings settings, SpaceTRS domain, float displacement, bool isPlane, JobHandle dependency) =>
            new SurfaceJob<TN>
            {
                _vertices = meshData.GetVertexData<SingleStream.Stream0>().Reinterpret<Vertex4>(12 * 4),
                _settings = settings,
                _domainTRS = domain.Matrix,
                _derivativeMatrix = domain.DerivativeMatrix,
                _displacement = displacement,
                _isPlane = isPlane
            }.ScheduleParallel(meshData.vertexCount / 4, resolution, dependency);
    }

    public delegate JobHandle SurfaceJobScheduleDelegate(Mesh.MeshData meshData, int resolution,
        Settings settings, SpaceTRS domain, float displacement, bool isPlane, JobHandle dependency);
}