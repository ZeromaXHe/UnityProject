using System.Collections;
using System.Collections.Generic;
using CatlikeCodings.ObjectManagement.Behaviors;
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
        [SerializeField] private ShapeFactory[] shapeFactories;
        [SerializeField] private float destroyDuration;

        private const int SaveVersion = 6;
        private Random.State _mainRandomState;
        private List<Shape> _shapes;
        private List<ShapeInstance> _killList, _markAsDyingList;
        private float _creationProgress, _destructionProgress;
        private int _loadedLevelBuildIndex;
        private bool _inGameUpdateLoop;
        private int _dyingShapeCount;

        public float CreationSpeed { get; set; }
        public float DestructionSpeed { get; set; }

        public static Game Instance { get; private set; }

        public void AddShape(Shape shape)
        {
            shape.SaveIndex = _shapes.Count;
            _shapes.Add(shape);
        }

        public Shape GetShape(int index)
        {
            return _shapes[index];
        }

        private void OnEnable()
        {
            Instance = this;
            if (shapeFactories[0].FactoryId != 0)
            {
                for (var i = 0; i < shapeFactories.Length; i++)
                {
                    shapeFactories[i].FactoryId = i;
                }
            }
        }

        private void Start()
        {
            _mainRandomState = Random.state;
            _shapes = new List<Shape>();
            _killList = new List<ShapeInstance>();
            _markAsDyingList = new List<ShapeInstance>();
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
                GameLevel.Current.SpawnShapes();
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
            _inGameUpdateLoop = true;
            foreach (var shape in _shapes)
            {
                shape.GameUpdate();
            }

            _inGameUpdateLoop = false;
            _creationProgress += Time.deltaTime * CreationSpeed;
            while (_creationProgress >= 1f)
            {
                _creationProgress -= 1f;
                GameLevel.Current.SpawnShapes();
            }

            _destructionProgress += Time.deltaTime * DestructionSpeed;
            while (_destructionProgress >= 1f)
            {
                _destructionProgress -= 1f;
                DestroyShape();
            }

            var limit = GameLevel.Current.PopulationLimit;
            if (limit > 0)
            {
                while (_shapes.Count - _dyingShapeCount > limit)
                {
                    DestroyShape();
                }
            }

            if (_killList.Count > 0)
            {
                foreach (var killShape in _killList)
                {
                    if (killShape.IsValid)
                    {
                        KillImmediately(killShape.Shape);
                    }
                }

                _killList.Clear();
            }

            if (_markAsDyingList.Count > 0)
            {
                foreach (var markAsDying in _markAsDyingList)
                {
                    if (markAsDying.IsValid)
                    {
                        MarkAsDyingImmediately(markAsDying.Shape);
                    }
                }

                _markAsDyingList.Clear();
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
            foreach (var shape in _shapes)
            {
                writer.Write(shape.OriginFactory.FactoryId);
                writer.Write(shape.ShapeId);
                writer.Write(shape.MaterialId);
                shape.Save(writer);
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
                var factoryId = version >= 5 ? reader.ReadInt() : 0;
                var shapeId = version > 0 ? reader.ReadInt() : 0;
                var materialId = version > 0 ? reader.ReadInt() : 0;
                var instance = shapeFactories[factoryId].Get(shapeId, materialId);
                instance.Load(reader);
            }

            foreach (var shape in _shapes)
            {
                shape.ResolveShapeInstances();
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
            foreach (var shape in _shapes)
            {
                shape.Recycle();
            }

            _shapes.Clear();
            _dyingShapeCount = 0;
        }

        private void DestroyShape()
        {
            if (_shapes.Count - _dyingShapeCount > 0)
            {
                var shape = _shapes[Random.Range(_dyingShapeCount, _shapes.Count)];
                if (destroyDuration <= 0f)
                {
                    KillImmediately(shape);
                }
                else
                {
                    shape.AddBehavior<DyingShapeBehavior>().Initialize(
                        shape, destroyDuration
                    );
                }
            }
        }

        public void Kill(Shape shape)
        {
            if (_inGameUpdateLoop)
            {
                _killList.Add(shape);
            }
            else
            {
                KillImmediately(shape);
            }
        }

        private void KillImmediately(Shape shape)
        {
            var index = shape.SaveIndex;
            shape.Recycle();
            if (index < _dyingShapeCount && index < --_dyingShapeCount)
            {
                _shapes[_dyingShapeCount].SaveIndex = index;
                _shapes[index] = _shapes[_dyingShapeCount];
                index = _dyingShapeCount;
            }

            var lastIndex = _shapes.Count - 1;
            if (index < lastIndex)
            {
                _shapes[lastIndex].SaveIndex = index;
                _shapes[index] = _shapes[lastIndex];
            }

            _shapes.RemoveAt(lastIndex);
        }

        private void MarkAsDyingImmediately(Shape shape)
        {
            var index = shape.SaveIndex;
            if (index < _dyingShapeCount)
            {
                return;
            }

            _shapes[_dyingShapeCount].SaveIndex = index;
            _shapes[index] = _shapes[_dyingShapeCount];
            shape.SaveIndex = _dyingShapeCount;
            _shapes[_dyingShapeCount++] = shape;
        }

        public void MarkAsDying(Shape shape)
        {
            if (_inGameUpdateLoop)
            {
                _markAsDyingList.Add(shape);
            }
            else
            {
                MarkAsDyingImmediately(shape);
            }
        }
        
        public bool IsMarkedAsDying (Shape shape) {
            return shape.SaveIndex < _dyingShapeCount;
        }
    }
}