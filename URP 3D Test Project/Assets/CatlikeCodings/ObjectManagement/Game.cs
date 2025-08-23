using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CatlikeCodings.ObjectManagement
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-22 22:02:58
    public class Game : PersistableObject
    {
        [SerializeField] private ShapeFactory shapeFactory;
        public KeyCode createKey = KeyCode.C;
        public KeyCode destroyKey = KeyCode.X;
        public KeyCode newGameKey = KeyCode.N;
        public KeyCode saveKey = KeyCode.S;
        public KeyCode loadKey = KeyCode.L;
        public PersistentStorage storage;
        public int levelCount;

        private const int SaveVersion = 2;
        private List<Shape> _shapes;
        private float _creationProgress, _destructionProgress;
        private int _loadedLevelBuildIndex;

        public static Game Instance { get; private set; }
        public float CreationSpeed { get; set; }
        public float DestructionSpeed { get; set; }
        public SpawnZone SpawnZoneOfLevel { get; set; }

        private void OnEnable()
        {
            Instance = this;
        }

        private void Start()
        {
            Instance = this;
            _shapes = new List<Shape>();
            if (Application.isEditor)
            {
                for (var i = 0; i < SceneManager.sceneCount; i++)
                {
                    var loadedScene = SceneManager.GetSceneAt(i);
                    if (loadedScene.name.Contains("Level "))
                    {
                        SceneManager.SetActiveScene(loadedScene);
                        _loadedLevelBuildIndex = loadedScene.buildIndex;
                        return;
                    }
                }
            }

            StartCoroutine(LoadLevel(1));
        }

        private IEnumerator LoadLevel(int levelBuildIndex)
        {
            enabled = false;
            if (_loadedLevelBuildIndex > 0)
            {
                yield return SceneManager.UnloadSceneAsync(_loadedLevelBuildIndex);
            }

            yield return SceneManager.LoadSceneAsync(levelBuildIndex, LoadSceneMode.Additive);
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(levelBuildIndex));
            _loadedLevelBuildIndex = levelBuildIndex;
            enabled = true;
        }

        private void Update()
        {
            if (Input.GetKeyDown(createKey))
            {
                CreateShape();
            }
            else if (Input.GetKeyDown(destroyKey))
            {
                DestroyShape();
            }
            else if (Input.GetKey(newGameKey))
            {
                BeginNewGame();
            }
            else if (Input.GetKeyDown(saveKey))
            {
                storage.Save(this, SaveVersion);
            }
            else if (Input.GetKeyDown(loadKey))
            {
                BeginNewGame();
                storage.Load(this);
            }
            else
            {
                for (var i = 1; i <= levelCount; i++)
                {
                    if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                    {
                        BeginNewGame();
                        StartCoroutine(LoadLevel(i));
                        return;
                    }
                }
            }

            _creationProgress += Time.deltaTime * CreationSpeed;
            while (_creationProgress >= 1f)
            {
                _creationProgress -= 1f;
                CreateShape();
            }

            _destructionProgress += Time.deltaTime * DestructionSpeed;
            while (_destructionProgress >= 1f)
            {
                _destructionProgress -= 1f;
                DestroyShape();
            }
        }

        public override void Save(GameDataWriter writer)
        {
            writer.Write(_shapes.Count);
            writer.Write(_loadedLevelBuildIndex);
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
            if (version > SaveVersion)
            {
                Debug.LogError("Unsupported future save version " + version);
                return;
            }

            var count = version <= 0 ? -version : reader.ReadInt();
            StartCoroutine(LoadLevel(version < 2 ? 1 : reader.ReadInt()));
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
                shapeFactory.Reclaim(obj);
            }

            _shapes.Clear();
        }

        private void CreateShape()
        {
            var instance = shapeFactory.GetRandom();
            var t = instance.transform;
            t.localPosition = SpawnZoneOfLevel.SpawnPoint;
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

        private void DestroyShape()
        {
            if (_shapes.Count > 0)
            {
                var index = Random.Range(0, _shapes.Count);
                shapeFactory.Reclaim(_shapes[index]);
                var lastIndex = _shapes.Count - 1;
                _shapes[index] = _shapes[lastIndex];
                _shapes.RemoveAt(lastIndex);
            }
        }
    }
}