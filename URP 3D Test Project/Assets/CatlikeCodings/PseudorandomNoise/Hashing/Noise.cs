using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace CatlikeCodings.PseudorandomNoise.Hashing
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-12 20:10:54
    public static partial class Noise
    {
        [Serializable]
        public struct Settings
        {
            public int seed;
            [Min(1)] public int frequency;
            [Range(1, 6)] public int octaves;
            [Range(2, 4)] public int lacunarity;
            [Range(0f, 1f)] public float persistence;

            public static Settings Default => new()
            {
                frequency = 4,
                octaves = 1,
                lacunarity = 2,
                persistence = 0.5f
            };
        }

        public interface INoise
        {
            Sample4 GetNoise4(float4x3 positions, SmallXxHash4 hash, int frequency);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Sample4 GetFractalNoise<TN>(float4x3 position, Settings settings) where TN : struct, INoise
        {
            var hash = SmallXxHash4.Seed(settings.seed);
            var frequency = settings.frequency;
            float amplitude = 1f, amplitudeSum = 0f;
            Sample4 sum = default;

            for (var o = 0; o < settings.octaves; o++)
            {
                sum += amplitude * default(TN).GetNoise4(position, hash + o, frequency);
                amplitudeSum += amplitude;
                frequency *= settings.lacunarity;
                amplitude *= settings.persistence;
            }

            return sum / amplitudeSum;
        }

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
        public struct Job<TN> : IJobFor where TN : struct, INoise
        {
            [ReadOnly] private NativeArray<float3x4> _positions;
            [WriteOnly] private NativeArray<float4> _noise;
            private Settings _settings;
            private float3x4 _domainTRS;

            public void Execute(int i) => _noise[i] =
                GetFractalNoise<TN>(_domainTRS.TransformVectors(transpose(_positions[i])), _settings).V;

            public static JobHandle ScheduleParallel(NativeArray<float3x4> positions, NativeArray<float4> noise,
                Settings settings, SpaceTRS domainTRS, int resolution, JobHandle dependency) =>
                new Job<TN>
                {
                    _positions = positions,
                    _noise = noise,
                    _settings = settings,
                    _domainTRS = domainTRS.Matrix,
                }.ScheduleParallel(positions.Length, resolution, dependency);
        }

        public delegate JobHandle ScheduleDelegate(NativeArray<float3x4> positions, NativeArray<float4> noise,
            Settings settings, SpaceTRS trs, int resolution, JobHandle dependency);
    }
}