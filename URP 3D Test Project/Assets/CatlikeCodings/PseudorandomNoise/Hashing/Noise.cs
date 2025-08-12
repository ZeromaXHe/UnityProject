using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace CatlikeCodings.PseudorandomNoise.Hashing
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-12 20:10:54
    public static partial class Noise
    {
        public interface INoise
        {
            float4 GetNoise4(float4x3 positions, SmallXxHash4 hash);
        }

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
        public struct Job<TN> : IJobFor where TN : struct, INoise
        {
            [ReadOnly] private NativeArray<float3x4> _positions;
            [WriteOnly] private NativeArray<float4> _noise;
            private SmallXxHash4 _hash;
            private float3x4 _domainTRS;

            public void Execute(int i)
            {
                _noise[i] = default(TN).GetNoise4(_domainTRS.TransformVectors(transpose(_positions[i])), _hash);
            }

            public static JobHandle ScheduleParallel(
                NativeArray<float3x4> positions, NativeArray<float4> noise,
                int seed, SpaceTRS domainTRS, int resolution, JobHandle dependency
            ) => new Job<TN>
            {
                _positions = positions,
                _noise = noise,
                _hash = SmallXxHash.Seed(seed),
                _domainTRS = domainTRS.Matrix,
            }.ScheduleParallel(positions.Length, resolution, dependency);
        }

        public delegate JobHandle ScheduleDelegate(NativeArray<float3x4> positions, NativeArray<float4> noise, int seed,
            SpaceTRS domainTRS, int resolution, JobHandle dependency);
    }
}