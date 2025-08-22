using UnityEngine;

namespace CatlikeCodings.ObjectManagement
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-22 22:44:03
    public class Shape : PersistableObject
    {
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

        private Color _color;
        private MeshRenderer _meshRenderer;

        private static MaterialPropertyBlock _sharedPropertyBlock;
        private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");

        private void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        public void SetMaterial(Material material, int materialId)
        {
            _meshRenderer.material = material;
            MaterialId = materialId;
        }

        public void SetColor(Color color)
        {
            _color = color;
            _sharedPropertyBlock ??= new MaterialPropertyBlock();
            _sharedPropertyBlock.SetColor(ColorPropertyId, color);
            _meshRenderer.SetPropertyBlock(_sharedPropertyBlock);
        }

        public override void Save(GameDataWriter writer)
        {
            base.Save(writer);
            writer.Write(_color);
        }

        public override void Load(GameDataReader reader)
        {
            base.Load(reader);
            SetColor(reader.Version > 0 ? reader.ReadColor() : Color.white);
        }
    }
}