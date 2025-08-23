using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        [SerializeField] private bool reseedOnLoad;
        [SerializeField] private Slider creationSpeedSlider;
        [SerializeField] private Slider destructionSpeedSlider;

        private const int SaveVersion = 4;
        private Random.State _mainRandomState;
        private List<Shape> _shapes;
        private float _creationProgress, _destructionProgress;
        private int _loadedLevelBuildIndex;

        public float CreationSpeed { get; set; }
        public float DestructionSpeed { get; set; }

        private void Start()
        {
            _mainRandomState = Random.state;
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

            BeginNewGame();
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
                StartCoroutine(LoadLevel(_loadedLevelBuildIndex));
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
        }

        private void FixedUpdate()
        {
            foreach (var shape in _shapes)
            {
                shape.GameUpdate();
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
            writer.Write(Random.state);
            writer.Write(CreationSpeed);
            writer.Write(_creationProgress);
            writer.Write(DestructionSpeed);
            writer.Write(_destructionProgress);
            writer.Write(_loadedLevelBuildIndex);
            GameLevel.Current.Save(writer);
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

            StartCoroutine(LoadGame(reader));
        }

        private IEnumerator LoadGame(GameDataReader reader)
        {
            var version = reader.Version;
            var count = version <= 0 ? -version : reader.ReadInt();
            if (version >= 3)
            {
                var state = reader.ReadRandomState();
                if (!reseedOnLoad)
                {
                    Random.state = state;
                }

                creationSpeedSlider.value = CreationSpeed = reader.ReadFloat();
                _creationProgress = reader.ReadFloat();
                destructionSpeedSlider.value = DestructionSpeed = reader.ReadFloat();
                _destructionProgress = reader.ReadFloat();
            }

            yield return LoadLevel(version < 2 ? 1 : reader.ReadInt());
            if (version >= 3)
            {
                GameLevel.Current.Load(reader);
            }

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
            Random.state = _mainRandomState;
            var seed = Random.Range(0, int.MaxValue) ^ (int)Time.unscaledTime;
            _mainRandomState = Random.state;
            Random.InitState(seed);
            creationSpeedSlider.value = CreationSpeed = 0;
            destructionSpeedSlider.value = DestructionSpeed = 0;
            foreach (var obj in _shapes)
            {
                shapeFactory.Reclaim(obj);
            }

            _shapes.Clear();
        }

        private void CreateShape()
        {
            var instance = shapeFactory.GetRandom();
            GameLevel.Current.ConfigureSpawn(instance);
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