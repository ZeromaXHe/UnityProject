using UnityEngine;

namespace CatlikeCodings.ObjectManagement
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-23 12:15:50
    public class CompositeSpawnZone : SpawnZone
    {
        [SerializeField] private bool overrideConfig;
        [SerializeField] private bool sequential;
        [SerializeField] private SpawnZone[] spawnZones;

        private int _nextSequentialIndex;

        public override Vector3 SpawnPoint
        {
            get
            {
                int index;
                if (sequential)
                {
                    index = _nextSequentialIndex++;
                    if (_nextSequentialIndex >= spawnZones.Length)
                    {
                        _nextSequentialIndex = 0;
                    }
                }
                else
                {
                    index = Random.Range(0, spawnZones.Length);
                }

                return spawnZones[index].SpawnPoint;
            }
        }

        public override void ConfigureSpawn(Shape shape)
        {
            if (overrideConfig)
            {
                base.ConfigureSpawn(shape);
            }
            else
            {
                int index;
                if (sequential)
                {
                    index = _nextSequentialIndex++;
                    if (_nextSequentialIndex >= spawnZones.Length)
                    {
                        _nextSequentialIndex = 0;
                    }
                }
                else
                {
                    index = Random.Range(0, spawnZones.Length);
                }

                spawnZones[index].ConfigureSpawn(shape);
            }
        }

        public override void Save(GameDataWriter writer)
        {
            writer.Write(_nextSequentialIndex);
        }

        public override void Load(GameDataReader reader)
        {
            _nextSequentialIndex = reader.ReadInt();
        }
    }
}