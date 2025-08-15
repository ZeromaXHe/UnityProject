using CatlikeCodings.PseudorandomNoise.Hashing;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;
using static Unity.Mathematics.math;
using static CatlikeCodings.PseudorandomNoise.Hashing.Noise;

namespace CatlikeCodings.PseudorandomSurfaces
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-15 15:02:15
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct FlowJob<TN> : IJobParticleSystemParallelForBatch where TN : struct, INoise
    {
        private Settings _settings;
        private float3x4 _domainTRS;
        private float3x3 _derivativeMatrix;
        private float _displacement;
        private bool _isPlane, _isCurl;

        public void Execute(ParticleSystemJobData data, int startIndex, int count)
        {
            if (count != 4)
            {
                return;
            }

            NativeSlice<float4>
                px = GetSlice(data.positions.x, startIndex),
                py = GetSlice(data.positions.y, startIndex),
                pz = GetSlice(data.positions.z, startIndex);
            NativeSlice<float4>
                vx = GetSlice(data.velocities.x, startIndex),
                vy = GetSlice(data.velocities.y, startIndex),
                vz = GetSlice(data.velocities.z, startIndex);
            var life = GetSlice(data.aliveTimePercent, startIndex);
            var p = float4x3(px[0], py[0], pz[0]);
            if (_isPlane)
            {
                p.c1 = 0f;
                life[0] = select(life[0], 100f, abs(p.c0) > 0.5f | abs(p.c2) > 0.5f);
            }
            else
            {
                p = p.NormalizeRows();
            }

            var noise = GetFractalNoise<TN>(_domainTRS.TransformVectors(p), _settings) * _displacement;
            noise.Derivatives = _derivativeMatrix.TransformVectors(noise.Derivatives);
            if (_isPlane)
            {
                if (_isCurl)
                {
                    vx[0] = noise.Dz;
                    vz[0] = -noise.Dx;
                }
                else
                {
                    vx[0] = -noise.Dx;
                    vz[0] = -noise.Dz;
                }

                py[0] = noise.V + 0.05f;
            }
            else
            {
                noise.V += 1f;
                noise.Dx /= noise.V;
                noise.Dy /= noise.V;
                noise.Dz /= noise.V;
                var pd = p.c0 * noise.Dx + p.c1 * noise.Dy + p.c2 * noise.Dz;
                noise.Dx -= pd * p.c0;
                noise.Dy -= pd * p.c1;
                noise.Dz -= pd * p.c2;
                if (_isCurl)
                {
                    vx[0] = p.c1 * noise.Dz - p.c2 * noise.Dy;
                    vy[0] = p.c2 * noise.Dx - p.c0 * noise.Dz;
                    vz[0] = p.c0 * noise.Dy - p.c1 * noise.Dx;
                }
                else
                {
                    vx[0] = -noise.Dx;
                    vy[0] = -noise.Dy;
                    vz[0] = -noise.Dz;
                }

                noise.V += 0.05f;
                px[0] = p.c0 * noise.V;
                py[0] = p.c1 * noise.V;
                pz[0] = p.c2 * noise.V;
            }
        }

        private static NativeSlice<float4> GetSlice(NativeArray<float> data, int i) =>
            data.Slice(i, 4).SliceConvert<float4>();

        public static JobHandle ScheduleParallel(ParticleSystem system, Settings settings, SpaceTRS domain,
            float displacement, bool isPlane, bool isCurl) => new FlowJob<TN>
        {
            _settings = settings,
            _domainTRS = domain.Matrix,
            _derivativeMatrix = domain.DerivativeMatrix,
            _displacement = displacement,
            _isPlane = isPlane,
            _isCurl = isCurl
        }.ScheduleBatch(system, 4);
    }

    public delegate JobHandle FlowJobScheduleDelegate(ParticleSystem system, Settings settings, SpaceTRS domain,
        float displacement, bool isPlane, bool isCurl);
}