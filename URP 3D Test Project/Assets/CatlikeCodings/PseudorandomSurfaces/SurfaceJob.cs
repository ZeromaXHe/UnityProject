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
        private float _displacement;
        private float3x3 _derivativeMatrix;

        public void Execute(int i)
        {
            var v = _vertices[i];
            var noise = GetFractalNoise<TN>(
                _domainTRS.TransformVectors(transpose(
                    float3x4(v.V0.position, v.V1.position, v.V2.position, v.V3.position))),
                _settings) * _displacement;
            v.V0.position.y = noise.V.x;
            v.V1.position.y = noise.V.y;
            v.V2.position.y = noise.V.z;
            v.V3.position.y = noise.V.w;
            var dNoise = _derivativeMatrix.TransformVectors(noise.Derivatives);
            var normalizer = rsqrt(dNoise.c0 * dNoise.c0 + 1f);
            var tangentY = dNoise.c0 * normalizer;
            v.V0.tangent = float4(normalizer.x, tangentY.x, 0f, -1f);
            v.V1.tangent = float4(normalizer.y, tangentY.y, 0f, -1f);
            v.V2.tangent = float4(normalizer.z, tangentY.z, 0f, -1f);
            v.V3.tangent = float4(normalizer.w, tangentY.w, 0f, -1f);
            normalizer = rsqrt(dNoise.c0 * dNoise.c0 + dNoise.c2 * dNoise.c2 + 1f);
            var normalX = -dNoise.c0 * normalizer;
            var normalZ = -dNoise.c2 * normalizer;
            v.V0.normal = float3(normalX.x, normalizer.x, normalZ.x);
            v.V1.normal = float3(normalX.y, normalizer.y, normalZ.y);
            v.V2.normal = float3(normalX.z, normalizer.z, normalZ.z);
            v.V3.normal = float3(normalX.w, normalizer.w, normalZ.w);
            _vertices[i] = v;
        }

        public static JobHandle ScheduleParallel(Mesh.MeshData meshData, int resolution,
            Settings settings, SpaceTRS domain, float displacement, JobHandle dependency) =>
            new SurfaceJob<TN>
            {
                _vertices = meshData.GetVertexData<SingleStream.Stream0>().Reinterpret<Vertex4>(12 * 4),
                _settings = settings,
                _domainTRS = domain.Matrix,
                _displacement = displacement,
                _derivativeMatrix = domain.DerivativeMatrix
            }.ScheduleParallel(meshData.vertexCount / 4, resolution, dependency);
    }

    public delegate JobHandle SurfaceJobScheduleDelegate(Mesh.MeshData meshData, int resolution,
        Settings settings, SpaceTRS domain, float displacement, JobHandle dependency);
}