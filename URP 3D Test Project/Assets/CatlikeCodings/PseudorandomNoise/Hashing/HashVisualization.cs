using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace CatlikeCodings.PseudorandomNoise.Hashing
{
    public class HashVisualization : MonoBehaviour
    {
        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
        private struct HashJob : IJobFor
        {
            [ReadOnly] public NativeArray<float3x4> Positions;
            [WriteOnly] public NativeArray<uint4> Hashes;
            public SmallXxHash4 Hash;
            public float3x4 DomainTRS;

            private static float4x3 TransformPositions(float3x4 trs, float4x3 p) => float4x3(
                trs.c0.x * p.c0 + trs.c1.x * p.c1 + trs.c2.x * p.c2 + trs.c3.x,
                trs.c0.y * p.c0 + trs.c1.y * p.c1 + trs.c2.y * p.c2 + trs.c3.y,
                trs.c0.z * p.c0 + trs.c1.z * p.c1 + trs.c2.z * p.c2 + trs.c3.z
            );

            public void Execute(int i)
            {
                var p = TransformPositions(DomainTRS, transpose(Positions[i]));
                var u = (int4)floor(p.c0);
                var v = (int4)floor(p.c1);
                var w = (int4)floor(p.c2);
                Hashes[i] = Hash.Eat(u).Eat(v).Eat(w);
            }
        }

        private static readonly int HashesId = Shader.PropertyToID("_Hashes"),
            PositionsId = Shader.PropertyToID("_Positions"),
            NormalsId = Shader.PropertyToID("_Normals"),
            ConfigId = Shader.PropertyToID("_Config");

        private enum Shape { Plane, Sphere, Torus }

        private static readonly Shapes.ScheduleDelegate[] ShapeJobs = {
            Shapes.Job<Shapes.Plane>.ScheduleParallel,
            Shapes.Job<Shapes.Sphere>.ScheduleParallel,
            Shapes.Job<Shapes.Torus>.ScheduleParallel
        };
        
        [SerializeField] private Mesh instanceMesh;
        [SerializeField] private Material material;
        [SerializeField] private Shape shape;
        [SerializeField, Range(0.1f, 10f)] private float instanceScale = 2f;
        [SerializeField, Range(1, 512)] private int resolution = 16;
        [SerializeField, Range(-0.5f, 0.5f)] private float displacement = 0.1f;
        [SerializeField] private int seed;
        [SerializeField] private SpaceTRS domain = new() { scale = 8f };

        private NativeArray<uint4> _hashes;
        private NativeArray<float3x4> _positions, _normals;
        private ComputeBuffer _hashesBuffer, _positionsBuffer, _normalsBuffer;
        private MaterialPropertyBlock _propertyBlock;

        private bool _isDirty;
        private Bounds _bounds;

        private void OnEnable()
        {
            _isDirty = true;
            var length = resolution * resolution;
            length = length / 4 + (length & 1);
            _hashes = new NativeArray<uint4>(length, Allocator.Persistent);
            _positions = new NativeArray<float3x4>(length, Allocator.Persistent);
            _normals = new NativeArray<float3x4>(length, Allocator.Persistent);
            _hashesBuffer = new ComputeBuffer(length * 4, 4);
            _positionsBuffer = new ComputeBuffer(length * 4, 3 * 4);
            _normalsBuffer = new ComputeBuffer(length * 4, 3 * 4);

            _propertyBlock ??= new MaterialPropertyBlock();
            _propertyBlock.SetBuffer(HashesId, _hashesBuffer);
            _propertyBlock.SetBuffer(PositionsId, _positionsBuffer);
            _propertyBlock.SetBuffer(NormalsId, _normalsBuffer);
            _propertyBlock.SetVector(ConfigId, new Vector4(resolution, instanceScale / resolution, displacement));
        }

        private void OnDisable()
        {
            _hashes.Dispose();
            _positions.Dispose();
            _normals.Dispose();
            _hashesBuffer.Release();
            _positionsBuffer.Release();
            _normalsBuffer.Release();
            _hashesBuffer = null;
            _positionsBuffer = null;
            _normalsBuffer = null;
        }

        private void OnValidate()
        {
            if (_hashesBuffer != null && enabled)
            {
                OnDisable();
                OnEnable();
            }
        }

        // Start is called before the first frame update
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
            if (_isDirty || transform.hasChanged)
            {
                _isDirty = false;
                transform.hasChanged = false;
                var handle = ShapeJobs[(int)shape](_positions, _normals, resolution,
                    transform.localToWorldMatrix, default);
                new HashJob
                {
                    Positions = _positions,
                    Hashes = _hashes,
                    Hash = SmallXxHash.Seed(seed),
                    DomainTRS = domain.Matrix
                }.ScheduleParallel(_hashes.Length, resolution, handle).Complete();
                _hashesBuffer.SetData(_hashes.Reinterpret<uint>(4 * 4));
                _positionsBuffer.SetData(_positions.Reinterpret<float3>(3 * 4 * 4));
                _normalsBuffer.SetData(_normals.Reinterpret<float3>(3 * 4 * 4));
                _bounds = new Bounds(
                    transform.position,
                    float3(2f * cmax(abs(transform.lossyScale)) + displacement)
                );
            }

            Graphics.DrawMeshInstancedProcedural(instanceMesh, 0, material, _bounds,
                resolution * resolution, _propertyBlock);
        }
    }
}