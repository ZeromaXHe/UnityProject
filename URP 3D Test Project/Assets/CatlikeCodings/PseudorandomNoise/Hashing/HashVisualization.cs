using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace CatlikeCodings.PseudorandomNoise.Hashing
{
    public class HashVisualization : Visualization
    {
        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
        private struct HashJob : IJobFor
        {
            [ReadOnly] public NativeArray<float3x4> Positions;
            [WriteOnly] public NativeArray<uint4> Hashes;
            public SmallXxHash4 Hash;
            public float3x4 DomainTRS;

            public void Execute(int i)
            {
                var p = DomainTRS.TransformVectors(transpose(Positions[i]));
                var u = (int4)floor(p.c0);
                var v = (int4)floor(p.c1);
                var w = (int4)floor(p.c2);
                Hashes[i] = Hash.Eat(u).Eat(v).Eat(w);
            }
        }

        private static readonly int HashesId = Shader.PropertyToID("_Hashes");

        [SerializeField] private int seed;
        [SerializeField] private SpaceTRS domain = new() { scale = 8f };

        private NativeArray<uint4> _hashes;
        private ComputeBuffer _hashesBuffer;

        protected override void EnableVisualization(int dataLength, MaterialPropertyBlock propertyBlock)
        {
            _hashes = new NativeArray<uint4>(dataLength, Allocator.Persistent);
            _hashesBuffer = new ComputeBuffer(dataLength * 4, 4);
            propertyBlock.SetBuffer(HashesId, _hashesBuffer);
        }

        protected override void DisableVisualization()
        {
            _hashes.Dispose();
            _hashesBuffer.Release();
            _hashesBuffer = null;
        }

        protected override void UpdateVisualization(NativeArray<float3x4> positions, int resolution, JobHandle handle)
        {
            new HashJob
            {
                Positions = positions,
                Hashes = _hashes,
                Hash = SmallXxHash.Seed(seed),
                DomainTRS = domain.Matrix
            }.ScheduleParallel(_hashes.Length, resolution, handle).Complete();
            _hashesBuffer.SetData(_hashes.Reinterpret<uint>(4 * 4));
        }
    }
}