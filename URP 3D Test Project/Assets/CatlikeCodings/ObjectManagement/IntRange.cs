using UnityEngine;

namespace CatlikeCodings.ObjectManagement
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-23 18:34:23
    [System.Serializable]
    public struct IntRange
    {
        public int min, max;

        public int RandomValueInRange => Random.Range(min, max + 1);
    }
}