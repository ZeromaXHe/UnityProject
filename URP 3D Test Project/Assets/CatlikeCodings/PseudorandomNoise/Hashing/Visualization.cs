using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace CatlikeCodings.PseudorandomNoise.Hashing
{
    public abstract class Visualization : MonoBehaviour
    {
        private static readonly int PositionsId = Shader.PropertyToID("_Positions"),
            NormalsId = Shader.PropertyToID("_Normals"),
            ConfigId = Shader.PropertyToID("_Config");

        private enum Shape
        {
            Plane,
            Sphere,
            Torus
        }

        private static readonly Shapes.ScheduleDelegate[] ShapeJobs =
        {
            Shapes.Job<Shapes.Plane>.ScheduleParallel, Shapes.Job<Shapes.Sphere>.ScheduleParallel,
            Shapes.Job<Shapes.Torus>.ScheduleParallel
        };

        [SerializeField] private Mesh instanceMesh;
        [SerializeField] private Material material;
        [SerializeField] private Shape shape;
        [SerializeField, Range(0.1f, 10f)] private float instanceScale = 2f;
        [SerializeField, Range(1, 512)] private int resolution = 16;
        [SerializeField, Range(-0.5f, 0.5f)] private float displacement = 0.1f;

        private NativeArray<float3x4> _positions, _normals;
        private ComputeBuffer _positionsBuffer, _normalsBuffer;
        private MaterialPropertyBlock _propertyBlock;

        private bool _isDirty;
        private Bounds _bounds;

        private void OnEnable()
        {
            _isDirty = true;
            var length = resolution * resolution;
            length = length / 4 + (length & 1);
            _positions = new NativeArray<float3x4>(length, Allocator.Persistent);
            _normals = new NativeArray<float3x4>(length, Allocator.Persistent);
            _positionsBuffer = new ComputeBuffer(length * 4, 3 * 4);
            _normalsBuffer = new ComputeBuffer(length * 4, 3 * 4);

            _propertyBlock ??= new MaterialPropertyBlock();
            EnableVisualization(length, _propertyBlock);
            _propertyBlock.SetBuffer(PositionsId, _positionsBuffer);
            _propertyBlock.SetBuffer(NormalsId, _normalsBuffer);
            _propertyBlock.SetVector(ConfigId, new Vector4(resolution, instanceScale / resolution, displacement));
        }

        private void OnDisable()
        {
            _positions.Dispose();
            _normals.Dispose();
            _positionsBuffer.Release();
            _normalsBuffer.Release();
            _positionsBuffer = null;
            _normalsBuffer = null;
            DisableVisualization();
        }

        private void OnValidate()
        {
            if (_positionsBuffer != null && enabled)
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
                UpdateVisualization(_positions, resolution, ShapeJobs[(int)shape](
                    _positions, _normals, resolution, transform.localToWorldMatrix, default));
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

        protected abstract void EnableVisualization(int dataLength, MaterialPropertyBlock propertyBlock);
        protected abstract void DisableVisualization();
        protected abstract void UpdateVisualization(NativeArray<float3x4> positions, int resolution, JobHandle handle);
    }
}