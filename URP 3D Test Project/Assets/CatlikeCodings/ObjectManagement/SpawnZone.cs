using UnityEngine;

namespace CatlikeCodings.ObjectManagement
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-23 11:57:07
    public abstract class SpawnZone : PersistableObject
    {
        public abstract Vector3 SpawnPoint { get; }
    }
}