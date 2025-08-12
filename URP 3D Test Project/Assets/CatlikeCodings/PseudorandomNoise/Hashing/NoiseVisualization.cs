using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static CatlikeCodings.PseudorandomNoise.Hashing.Noise;

namespace CatlikeCodings.PseudorandomNoise.Hashing
{
    public class NoiseVisualization : Visualization
    {
        private static readonly ScheduleDelegate[,] NoiseJobs =
        {
            {
                Job<Lattice1D<Perlin>>.ScheduleParallel,
                Job<Lattice2D<Perlin>>.ScheduleParallel,
                Job<Lattice3D<Perlin>>.ScheduleParallel
            },
            {
                Job<Lattice1D<Value>>.ScheduleParallel,
                Job<Lattice2D<Value>>.ScheduleParallel,
                Job<Lattice3D<Value>>.ScheduleParallel
            }
        };

        private static readonly int NoiseId = Shader.PropertyToID("_Noise");

        private enum NoiseType
        {
            Perlin,
            Value
        }

        [SerializeField] private NoiseType type;
        [SerializeField] private int seed;
        [SerializeField, Range(1, 3)] private int dimensions = 3;
        [SerializeField] private SpaceTRS domain = new() { scale = 8f };

        private NativeArray<float4> _noise;
        private ComputeBuffer _noiseBuffer;

        protected override void EnableVisualization(int dataLength, MaterialPropertyBlock propertyBlock)
        {
            _noise = new NativeArray<float4>(dataLength, Allocator.Persistent);
            _noiseBuffer = new ComputeBuffer(dataLength * 4, 4);
            propertyBlock.SetBuffer(NoiseId, _noiseBuffer);
        }

        protected override void DisableVisualization()
        {
            _noise.Dispose();
            _noiseBuffer.Release();
            _noiseBuffer = null;
        }

        protected override void UpdateVisualization(NativeArray<float3x4> positions, int resolution, JobHandle handle)
        {
            NoiseJobs[(int)type, dimensions - 1](positions, _noise, seed, domain, resolution, handle).Complete();
            _noiseBuffer.SetData(_noise.Reinterpret<float>(4 * 4));
        }
    }
}