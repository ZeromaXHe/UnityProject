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
        public PersistableObject prefab;
        public KeyCode createKey = KeyCode.C;
        public KeyCode newGameKey = KeyCode.N;
        public KeyCode saveKey = KeyCode.S;
        public KeyCode loadKey = KeyCode.L;
        public PersistentStorage storage;

        private List<PersistableObject> _objects;

        private void Awake()
        {
            _objects = new List<PersistableObject>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(createKey))
            {
                CreateObject();
            }
            else if (Input.GetKey(newGameKey))
            {
                BeginNewGame();
            }
            else if (Input.GetKeyDown(saveKey))
            {
                storage.Save(this);
            }
            else if (Input.GetKeyDown(loadKey))
            {
                BeginNewGame();
                storage.Load(this);
            }
        }

        public override void Save(GameDataWriter writer)
        {
            writer.Write(_objects.Count);
            foreach (var o in _objects)
            {
                o.Save(writer);
            }
        }

        public override void Load(GameDataReader reader)
        {
            var count = reader.ReadInt();
            for (var i = 0; i < count; i++)
            {
                var o = Instantiate(prefab);
                o.Load(reader);
                _objects.Add(o);
            }
        }

        private void BeginNewGame()
        {
            foreach (var obj in _objects)
            {
                Destroy(obj.gameObject);
            }

            _objects.Clear();
        }

        private void CreateObject()
        {
            var o = Instantiate(prefab);
            var t = o.transform;
            t.localPosition = Random.insideUnitSphere * 5f;
            t.localRotation = Random.rotation;
            t.localScale = Vector3.one * Random.Range(0.1f, 1f);
            _objects.Add(o);
        }
    }
}