using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CatlikeCodings.ObjectManagement
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-22 22:02:58
    public class Game : PersistableObject
    {
        public ShapeFactory shapeFactory;
        public KeyCode createKey = KeyCode.C;
        public KeyCode newGameKey = KeyCode.N;
        public KeyCode saveKey = KeyCode.S;
        public KeyCode loadKey = KeyCode.L;
        public PersistentStorage storage;

        private List<Shape> _shapes;
        const int saveVersion = 1;

        private void Awake()
        {
            _shapes = new List<Shape>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(createKey))
            {
                CreateShape();
            }
            else if (Input.GetKey(newGameKey))
            {
                BeginNewGame();
            }
            else if (Input.GetKeyDown(saveKey))
            {
                storage.Save(this, saveVersion);
            }
            else if (Input.GetKeyDown(loadKey))
            {
                BeginNewGame();
                storage.Load(this);
            }
        }

        public override void Save(GameDataWriter writer)
        {
            writer.Write(_shapes.Count);
            foreach (var o in _shapes)
            {
                writer.Write(o.ShapeId);
                writer.Write(o.MaterialId);
                o.Save(writer);
            }
        }

        public override void Load(GameDataReader reader)
        {
            var version = reader.Version;
            if (version > saveVersion)
            {
                Debug.LogError("Unsupported future save version " + version);
                return;
            }

            var count = version <= 0 ? -version : reader.ReadInt();
            for (var i = 0; i < count; i++)
            {
                var shapeId = version > 0 ? reader.ReadInt() : 0;
                var materialId = version > 0 ? reader.ReadInt() : 0;
                var instance = shapeFactory.Get(shapeId, materialId);
                instance.Load(reader);
                _shapes.Add(instance);
            }
        }

        private void BeginNewGame()
        {
            foreach (var obj in _shapes)
            {
                Destroy(obj.gameObject);
            }

            _shapes.Clear();
        }

        private void CreateShape()
        {
            var instance = shapeFactory.GetRandom();
            var t = instance.transform;
            t.localPosition = Random.insideUnitSphere * 5f;
            t.localRotation = Random.rotation;
            t.localScale = Vector3.one * Random.Range(0.1f, 1f);
            instance.SetColor(Random.ColorHSV(
                hueMin: 0f, hueMax: 1f,
                saturationMin: 0.5f, saturationMax: 1f,
                valueMin: 0.25f, valueMax: 1f,
                alphaMin: 1f, alphaMax: 1f
            ));
            _shapes.Add(instance);
        }
    }
}