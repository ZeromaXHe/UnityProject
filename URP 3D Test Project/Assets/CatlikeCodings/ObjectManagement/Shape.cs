using System;
using System.Collections.Generic;
using CatlikeCodings.ObjectManagement.Behaviors;
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
        private readonly List<ShapeBehavior> _behaviorList = new();
        public float Age { get; private set; }
        public int InstanceId { get; private set; }
        public int SaveIndex { get; set; }
        public bool IsMarkedAsDying => Game.Instance.IsMarkedAsDying(this);

        private static MaterialPropertyBlock _sharedPropertyBlock;
        private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");

        private void Awake()
        {
            _colors = new Color[meshRenderers.Length];
        }

        public void GameUpdate()
        {
            Age += Time.deltaTime;
            for (var i = 0; i < _behaviorList.Count; i++)
            {
                if (!_behaviorList[i].GameUpdate(this))
                {
                    _behaviorList[i].Recycle();
                    _behaviorList.RemoveAt(i--);
                }
            }
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

            writer.Write(Age);
            writer.Write(_behaviorList.Count);
            foreach (var behavior in _behaviorList)
            {
                writer.Write((int)behavior.BehaviorType);
                behavior.Save(writer);
            }
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

            if (reader.Version >= 6)
            {
                Age = reader.ReadFloat();
                var behaviorCount = reader.ReadInt();
                for (var i = 0; i < behaviorCount; i++)
                {
                    var behavior = ((ShapeBehaviorType)reader.ReadInt()).GetInstance();
                    _behaviorList.Add(behavior);
                    behavior.Load(reader);
                }
            }
            else if (reader.Version >= 4)
            {
                AddBehavior<RotationShapeBehavior>().AngularVelocity = reader.ReadVector3();
                AddBehavior<MovementShapeBehavior>().Velocity = reader.ReadVector3();
            }
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
            Age = 0f;
            InstanceId += 1;
            foreach (var behavior in _behaviorList)
            {
                behavior.Recycle();
            }

            _behaviorList.Clear();
            OriginFactory.Reclaim(this);
        }

        public T AddBehavior<T>() where T : ShapeBehavior, new()
        {
            var behavior = ShapeBehaviorPool<T>.Get();
            _behaviorList.Add(behavior);
            return behavior;
        }

        public void ResolveShapeInstances()
        {
            foreach (var shapeBehavior in _behaviorList)
            {
                shapeBehavior.ResolveShapeInstances();
            }
        }

        public void Die()
        {
            Game.Instance.Kill(this);
        }

        public void MarkAsDying()
        {
            Game.Instance.MarkAsDying(this);
        }
    }
}