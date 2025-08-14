using Unity.Mathematics;

namespace CatlikeCodings.PseudorandomNoise.Hashing
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-12 16:41:12
    [System.Serializable]
    public struct SpaceTRS
    {
        public float3 translation, rotation, scale;
        public float3x3 DerivativeMatrix =>
            math.mul(float3x3.EulerYXZ(-math.radians(rotation)), float3x3.Scale(scale));

        public float3x4 Matrix
        {
            get
            {
                var m = math.mul(
                    float3x3.Scale(scale), float3x3.EulerZXY(math.radians(rotation))
                );
                return math.float3x4(m.c0, m.c1, m.c2, translation);
            }
        }
    }
}