using UnityEngine;
using UnityEngine.Serialization;

namespace CatlikeCodings.ObjectManagement
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-23 12:07:35
    public partial class GameLevel : PersistableObject
    {
        [SerializeField] private int populationLimit;
        [SerializeField] private SpawnZone spawnZone;

        [FormerlySerializedAs("persistentObjects")] [SerializeField]
        private GameLevelObject[] levelObjects;

        public int PopulationLimit => populationLimit;
        public static GameLevel Current { get; private set; }

        private void OnEnable()
        {
            Current = this;
            if (levelObjects == null)
            {
                levelObjects = new GameLevelObject[0];
            }
        }

        public void SpawnShapes()
        {
            spawnZone.SpawnShapes();
        }

        public override void Save(GameDataWriter writer)
        {
            writer.Write(levelObjects.Length);
            foreach (var obj in levelObjects)
            {
                obj.Save(writer);
            }
        }

        public override void Load(GameDataReader reader)
        {
            var savedCount = reader.ReadInt();
            for (var i = 0; i < savedCount; i++)
            {
                levelObjects[i].Load(reader);
            }
        }

        public void GameUpdate()
        {
            foreach (var obj in levelObjects)
            {
                obj.GameUpdate();
            }
        }
    }
}