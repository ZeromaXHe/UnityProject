using UnityEngine;

namespace CatlikeCodings.Basics.Fractals
{
    public class Fractal : MonoBehaviour
    {
        [SerializeField, Range(1, 8)] private int depth = 4;
        [SerializeField] private Mesh mesh;
        [SerializeField] private Material material;

        private static readonly Vector3[] Directions =
        {
            Vector3.up, Vector3.right, Vector3.left, Vector3.forward, Vector3.back
        };

        private static readonly Quaternion[] Rotations =
        {
            Quaternion.identity, Quaternion.Euler(0f, 0f, -90f), Quaternion.Euler(0f, 0f, 90f),
            Quaternion.Euler(90f, 0f, 0f), Quaternion.Euler(-90f, 0f, 0f)
        };

        private struct FractalPart
        {
            public Vector3 Direction, WorldPosition;
            public Quaternion Rotation, WorldRotation;
            public float SpinAngle;
        }

        private static FractalPart CreatePart(int childIndex) =>
            new()
            {
                Direction = Directions[childIndex],
                Rotation = Rotations[childIndex],
            };

        private static readonly int MatricesId = Shader.PropertyToID("_Matrices");
        private static MaterialPropertyBlock _propertyBlock;
        private FractalPart[][] _parts;
        private Matrix4x4[][] _matrices;
        private ComputeBuffer[] _matricesBuffers;


        private void OnEnable()
        {
            _parts = new FractalPart[depth][];
            _matrices = new Matrix4x4[depth][];
            _matricesBuffers = new ComputeBuffer[depth];
            const int stride = 16 * 4;
            for (int i = 0, length = 1; i < _parts.Length; i++, length *= 5)
            {
                _parts[i] = new FractalPart[length];
                _matrices[i] = new Matrix4x4[length];
                _matricesBuffers[i] = new ComputeBuffer(length, stride);
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
            }

            _parts = null;
            _matrices = null;
            _matricesBuffers = null;
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
            var spinAngleDelta = 22.5f * Time.deltaTime;
            var rootPart = _parts[0][0];
            rootPart.SpinAngle += spinAngleDelta;
            rootPart.WorldRotation =
                transform.rotation * (rootPart.Rotation * Quaternion.Euler(0f, rootPart.SpinAngle, 0f));
            rootPart.WorldPosition = transform.position;
            _parts[0][0] = rootPart;
            var objectScale = transform.lossyScale.x;
            _matrices[0][0] = Matrix4x4.TRS(rootPart.WorldPosition, rootPart.WorldRotation, objectScale * Vector3.one);
            var scale = objectScale;
            for (var li = 1; li < _parts.Length; li++)
            {
                scale *= 0.5f;
                var parentParts = _parts[li - 1];
                var levelParts = _parts[li];
                var levelMatrices = _matrices[li];
                for (var fpi = 0; fpi < levelParts.Length; fpi++)
                {
                    var parent = parentParts[fpi / 5];
                    var part = levelParts[fpi];
                    part.SpinAngle += spinAngleDelta;
                    part.WorldRotation =
                        parent.WorldRotation * (part.Rotation * Quaternion.Euler(0f, part.SpinAngle, 0f));
                    part.WorldPosition = parent.WorldPosition + parent.WorldRotation * (1.5f * scale * part.Direction);
                    levelParts[fpi] = part;
                    levelMatrices[fpi] = Matrix4x4.TRS(part.WorldPosition, part.WorldRotation, scale * Vector3.one);
                }
            }

            var bounds = new Bounds(rootPart.WorldPosition, 3f * objectScale * Vector3.one);
            for (var i = 0; i < _matricesBuffers.Length; i++)
            {
                var buffer = _matricesBuffers[i];
                buffer.SetData(_matrices[i]);
                _propertyBlock.SetBuffer(MatricesId, buffer);
                Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, buffer.count, _propertyBlock);
            }
        }
    }
}