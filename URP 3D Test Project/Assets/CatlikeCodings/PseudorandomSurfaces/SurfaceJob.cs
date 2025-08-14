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

        public void Execute(int i)
        {
            var v = _vertices[i];
            var p = _domainTRS.TransformVectors(transpose(float3x4(
                v.V0.position, v.V1.position, v.V2.position, v.V3.position
            )));
            var hash = SmallXxHash4.Seed(_settings.seed);
            var frequency = _settings.frequency;
            float amplitude = 1f, amplitudeSum = 0f;
            float4 sum = 0f;

            for (var o = 0; o < _settings.octaves; o++)
            {
                sum += amplitude * default(TN).GetNoise4(p, hash + o, frequency);
                amplitudeSum += amplitude;
                frequency *= _settings.lacunarity;
                amplitude *= _settings.persistence;
            }

            var noise = sum / amplitudeSum;
            noise *= _displacement;
            v.V0.position.y = noise.x;
            v.V1.position.y = noise.y;
            v.V2.position.y = noise.z;
            v.V3.position.y = noise.w;
            _vertices[i] = v;
        }

        public static JobHandle ScheduleParallel(Mesh.MeshData meshData, int resolution,
            Settings settings, SpaceTRS domain, float displacement, JobHandle dependency) =>
            new SurfaceJob<TN>
            {
                _vertices = meshData.GetVertexData<SingleStream.Stream0>().Reinterpret<Vertex4>(12 * 4),
                _settings = settings,
                _domainTRS = domain.Matrix,
                _displacement = displacement
            }.ScheduleParallel(meshData.vertexCount / 4, resolution, dependency);
    }
}