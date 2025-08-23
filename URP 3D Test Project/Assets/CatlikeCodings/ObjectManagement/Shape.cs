using UnityEngine;

namespace CatlikeCodings.ObjectManagement
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-22 22:44:03
    public class Shape : PersistableObject
    {
        [SerializeField] private MeshRenderer[] meshRenderers;
        private int _shapeId = int.MinValue;

        public int ShapeId
        {
            get => _shapeId;
            set
            {
                if (_shapeId == int.MinValue && value != int.MinValue)
                {
                    _shapeId = value;
                }
                else
                {
                    Debug.LogError("Not allowed to change shapeId.");
                }
            }
        }

        public int MaterialId { get; private set; }
        public Vector3 AngularVelocity { get; set; }
        public Vector3 Velocity { get; set; }

        private Color[] _colors;

        public int ColorCount => _colors.Length;

        public ShapeFactory OriginFactory
        {
            get => _originFactory;
            set
            {
                if (_originFactory == null)
                {
                    _originFactory = value;
                }
                else
                {
                    Debug.LogError("Not allowed to change origin factory.");
                }
            }
        }

        private ShapeFactory _originFactory;

        private static MaterialPropertyBlock _sharedPropertyBlock;
        private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");

        private void Awake()
        {
            _colors = new Color[meshRenderers.Length];
        }

        public void GameUpdate()
        {
            transform.Rotate(AngularVelocity * Time.deltaTime);
            transform.localPosition += Velocity * Time.deltaTime;
        }

        public void SetMaterial(Material material, int materialId)
        {
            foreach (var meshRenderer in meshRenderers)
            {
                meshRenderer.material = material;
            }

            MaterialId = materialId;
        }

        public void SetColor(Color color)
        {
            _sharedPropertyBlock ??= new MaterialPropertyBlock();
            _sharedPropertyBlock.SetColor(ColorPropertyId, color);
            for (var i = 0; i < meshRenderers.Length; i++)
            {
                _colors[i] = color;
                meshRenderers[i].SetPropertyBlock(_sharedPropertyBlock);
            }
        }

        public void SetColor(Color color, int index)
        {
            _sharedPropertyBlock ??= new MaterialPropertyBlock();
            _sharedPropertyBlock.SetColor(ColorPropertyId, color);
            _colors[index] = color;
            meshRenderers[index].SetPropertyBlock(_sharedPropertyBlock);
        }

        public override void Save(GameDataWriter writer)
        {
            base.Save(writer);
            writer.Write(_colors.Length);
            foreach (var color in _colors)
            {
                writer.Write(color);
            }

            writer.Write(AngularVelocity);
            writer.Write(Velocity);
        }

        public override void Load(GameDataReader reader)
        {
            base.Load(reader);
            if (reader.Version >= 5)
            {
                LoadColors(reader);
            }
            else
            {
                SetColor(reader.Version > 0 ? reader.ReadColor() : Color.white);
            }

            AngularVelocity = reader.Version >= 4 ? reader.ReadVector3() : Vector3.zero;
            Velocity = reader.Version >= 4 ? reader.ReadVector3() : Vector3.zero;
        }

        private void LoadColors(GameDataReader reader)
        {
            var count = reader.ReadInt();
            var max = count <= _colors.Length ? count : _colors.Length;
            var i = 0;
            for (; i < max; i++)
            {
                SetColor(reader.ReadColor(), i);
            }

            if (count > _colors.Length)
            {
                for (; i < count; i++)
                {
                    reader.ReadColor();
                }
            }
            else if (count < _colors.Length)
            {
                for (; i < _colors.Length; i++)
                {
                    SetColor(Color.white, i);
                }
            }
        }

        public void Recycle()
        {
            OriginFactory.Reclaim(this);
        }
    }
}