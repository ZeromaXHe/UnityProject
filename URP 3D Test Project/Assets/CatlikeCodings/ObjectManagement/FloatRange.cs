using UnityEngine;

namespace CatlikeCodings.ObjectManagement
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-23 14:38:23
    [System.Serializable]
    public struct FloatRange
    {
        public float min, max;
        public float RandomValueInRange => Random.Range(min, max);
    }
}