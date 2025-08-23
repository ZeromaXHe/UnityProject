using UnityEngine;

namespace CatlikeCodings.ObjectManagement
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-23 12:15:50
    public class CompositeSpawnZone : SpawnZone
    {
        [SerializeField] private SpawnZone[] spawnZones;

        public override Vector3 SpawnPoint
        {
            get
            {
                var index = Random.Range(0, spawnZones.Length);
                return spawnZones[index].SpawnPoint;
            }
        }
    }
}