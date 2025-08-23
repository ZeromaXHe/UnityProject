using UnityEngine;

namespace CatlikeCodings.ObjectManagement
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-23 15:02:23
    [System.Serializable]
    public struct ColorRangeHSV
    {
        [FloatRangeSlider(0f, 1f)] public FloatRange hue, saturation, value;

        public Color RandomInRange =>
            Random.ColorHSV(
                hue.min, hue.max,
                saturation.min, saturation.max,
                value.min, value.max,
                1f, 1f
            );
    }
}