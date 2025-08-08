using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;
using quaternion = Unity.Mathematics.quaternion;
using Random = UnityEngine.Random;

namespace CatlikeCodings.Basics.Fractals
{
    public class FractalJob : MonoBehaviour
    {
        [SerializeField, Range(3, 8)] private int depth = 4;
        [SerializeField] private Mesh mesh, leafMesh;
        [SerializeField] private Material material;
        [SerializeField] private Gradient gradientA, gradientB;
        [SerializeField] private Color leafColorA, leafColorB;
        [SerializeField, Range(0f, 90f)] private float maxSagAngleA = 15f, maxSagAngleB = 25f;
        [SerializeField, Range(0f, 90f)] private float spinSpeedA = 20f, spinSpeedB = 25f;
        [SerializeField, Range(0f, 1f)] private float reverseSpinChance = 0.25f;

        private static readonly Quaternion[] Rotations =
        {
            Quaternion.identity, quaternion.RotateZ(-0.5f * PI), quaternion.RotateZ(0.5f * PI),
            quaternion.RotateX(0.5f * PI), quaternion.RotateX(-0.5f * PI)
        };

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
        private struct UpdateFractalLevelJob : IJobFor
        {
            public float Scale;
            public float DeltaTime;
            [ReadOnly] public NativeArray<FractalPart> Parents;
            public NativeArray<FractalPart> Parts;
            [WriteOnly] public NativeArray<float3x4> Matrices;

            public void Execute(int i)
            {
                var parent = Parents[i / 5];
                var part = Parts[i];
                part.SpinAngle += part.SpinVelocity * DeltaTime;
                var upAxis = mul(mul(parent.WorldRotation, part.Rotation), up());
                var sagAxis = cross(up(), upAxis);
                var sagMagnitude = length(sagAxis);
                quaternion baseRotation;
                if (sagMagnitude > 0f)
                {
                    sagAxis /= sagMagnitude;
                    var sagRotation = quaternion.AxisAngle(sagAxis, part.MaxSagAngle * sagMagnitude);
                    baseRotation = mul(sagRotation, parent.WorldRotation);
                }
                else
                {
                    baseRotation = parent.WorldRotation;
                }

                part.WorldRotation = mul(baseRotation, mul(part.Rotation, quaternion.RotateY(part.SpinAngle)));
                part.WorldPosition =
                    parent.WorldPosition + (Vector3)mul(part.WorldRotation, float3(0f, 1.5f * Scale, 0f));
                Parts[i] = part;
                var r = float3x3(part.WorldRotation) * Scale;
                Matrices[i] = float3x4(r.c0, r.c1, r.c2, part.WorldPosition);
            }
        }

        private struct FractalPart
        {
            public Vector3 WorldPosition;
            public Quaternion Rotation, WorldRotation;
            public float MaxSagAngle, SpinAngle, SpinVelocity;
        }

        private FractalPart CreatePart(int childIndex) =>
            new()
            {
                MaxSagAngle = radians(Random.Range(maxSagAngleA, maxSagAngleB)),
                Rotation = Rotations[childIndex],
                SpinVelocity =
                    (Random.value < reverseSpinChance ? -1f : 1f) * radians(Random.Range(spinSpeedA, spinSpeedB))
            };

        private static readonly int ColorAId = Shader.PropertyToID("_ColorA"),
            ColorBId = Shader.PropertyToID("_ColorB"),
            MatricesId = Shader.PropertyToID("_Matrices"),
            SequenceNumbersId = Shader.PropertyToID("_SequenceNumbers");

        private static MaterialPropertyBlock _propertyBlock;
        private NativeArray<FractalPart>[] _parts;
        private NativeArray<float3x4>[] _matrices;
        private ComputeBuffer[] _matricesBuffers;
        private Vector4[] _sequenceNumbers;


        private void OnEnable()
        {
            _parts = new NativeArray<FractalPart>[depth];
            _matrices = new NativeArray<float3x4>[depth];
            _matricesBuffers = new ComputeBuffer[depth];
            _sequenceNumbers = new Vector4[depth];
            const int stride = 12 * 4;
            for (int i = 0, length = 1; i < _parts.Length; i++, length *= 5)
            {
                _parts[i] = new NativeArray<FractalPart>(length, Allocator.Persistent);
                _matrices[i] = new NativeArray<float3x4>(length, Allocator.Persistent);
                _matricesBuffers[i] = new ComputeBuffer(length, stride);
                _sequenceNumbers[i] = new Vector4(Random.value, Random.value, Random.value, Random.value);
            }

            _parts[0][0] = CreatePart(0);
            for (var li = 1; li < _parts.Length; li++)
            {
                var levelParts = _parts[li];
                for (var fpi = 0; fpi < levelParts.Length; fpi += 5)
                {
                    for (var ci = 0; ci < 5; ci++)
                    {
                        levelParts[fpi + ci] = CreatePart(ci);
                    }
                }
            }

            _propertyBlock ??= new MaterialPropertyBlock();
        }

        private void OnDisable()
        {
            for (var i = 0; i < _matricesBuffers.Length; i++)
            {
                _matricesBuffers[i].Release();
                _parts[i].Dispose();
                _matrices[i].Dispose();
            }

            _parts = null;
            _matrices = null;
            _matricesBuffers = null;
            _sequenceNumbers = null;
        }

        private void OnValidate()
        {
            if (_parts != null && enabled)
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
            var deltaTime = Time.deltaTime;
            var rootPart = _parts[0][0];
            rootPart.SpinAngle += rootPart.SpinVelocity * deltaTime;
            rootPart.WorldRotation = mul(transform.rotation,
                mul(rootPart.Rotation, quaternion.RotateY(rootPart.SpinAngle)));
            rootPart.WorldPosition = transform.position;
            _parts[0][0] = rootPart;
            var objectScale = transform.lossyScale.x;
            var r = float3x3(rootPart.WorldRotation) * objectScale;
            _matrices[0][0] = float3x4(r.c0, r.c1, r.c2, rootPart.WorldPosition);
            var scale = objectScale;
            JobHandle jobHandle = default;
            for (var li = 1; li < _parts.Length; li++)
            {
                scale *= 0.5f;
                jobHandle = new UpdateFractalLevelJob
                {
                    DeltaTime = deltaTime,
                    Scale = scale,
                    Parents = _parts[li - 1],
                    Parts = _parts[li],
                    Matrices = _matrices[li]
                }.ScheduleParallel(_parts[li].Length, 5, jobHandle);
            }

            jobHandle.Complete();

            var bounds = new Bounds(rootPart.WorldPosition, 3f * objectScale * Vector3.one);
            var leafIndex = _matricesBuffers.Length - 1;
            for (var i = 0; i < _matricesBuffers.Length; i++)
            {
                var buffer = _matricesBuffers[i];
                buffer.SetData(_matrices[i]);
                Color colorA, colorB;
                Mesh instanceMesh;
                if (i == leafIndex)
                {
                    colorA = leafColorA;
                    colorB = leafColorB;
                    instanceMesh = leafMesh;
                }
                else
                {
                    var gradientInterpolator = i / (_matricesBuffers.Length - 2f);
                    colorA = gradientA.Evaluate(gradientInterpolator);
                    colorB = gradientB.Evaluate(gradientInterpolator);
                    instanceMesh = mesh;
                }

                _propertyBlock.SetColor(ColorAId, colorA);
                _propertyBlock.SetColor(ColorBId, colorB);
                _propertyBlock.SetBuffer(MatricesId, buffer);
                _propertyBlock.SetVector(SequenceNumbersId, _sequenceNumbers[i]);
                Graphics.DrawMeshInstancedProcedural(instanceMesh, 0, material, bounds, buffer.count, _propertyBlock);
            }
        }
    }
}