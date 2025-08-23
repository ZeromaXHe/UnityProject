using UnityEngine;

namespace CatlikeCodings.ObjectManagement
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-23 12:07:35
    public class GameLevel : PersistableObject
    {
        [SerializeField] private int populationLimit;
        [SerializeField] private SpawnZone spawnZone;
        [SerializeField] private PersistableObject[] persistentObjects;

        public int PopulationLimit => populationLimit;
        public static GameLevel Current { get; private set; }

        private void OnEnable()
        {
            Current = this;
            if (persistentObjects == null)
            {
                persistentObjects = new PersistableObject[0];
            }
        }

        public void SpawnShapes()
        {
            spawnZone.SpawnShapes();
        }

        public override void Save(GameDataWriter writer)
        {
            writer.Write(persistentObjects.Length);
            foreach (var obj in persistentObjects)
            {
                obj.Save(writer);
            }
        }

        public override void Load(GameDataReader reader)
        {
            var savedCount = reader.ReadInt();
            for (var i = 0; i < savedCount; i++)
            {
                persistentObjects[i].Load(reader);
            }
        }
    }
}