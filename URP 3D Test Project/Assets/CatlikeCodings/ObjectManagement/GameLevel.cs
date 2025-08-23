using UnityEngine;

namespace CatlikeCodings.ObjectManagement
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-23 12:07:35
    public class GameLevel : MonoBehaviour
    {
        [SerializeField] private SpawnZone spawnZone;

        private void Start()
        {
            Game.Instance.SpawnZoneOfLevel = spawnZone;
        }
    }
}