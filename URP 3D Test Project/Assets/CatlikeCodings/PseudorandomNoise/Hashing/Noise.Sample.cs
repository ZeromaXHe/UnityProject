using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace CatlikeCodings.PseudorandomNoise.Hashing
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-14 23:51:41
    public static partial class Noise
    {
        public struct Sample4
        {
            public float4 V, Dx, Dy, Dz;
            public float4x3 Derivatives => float4x3(Dx, Dy, Dz);
            public static implicit operator Sample4(float4 v) => new() { V = v };

            public static Sample4 operator +(Sample4 a, Sample4 b) => new()
            {
                V = a.V + b.V,
                Dx = a.Dx + b.Dx,
                Dy = a.Dy + b.Dy,
                Dz = a.Dz + b.Dz
            };

            public static Sample4 operator -(Sample4 a, Sample4 b) => new()
            {
                V = a.V - b.V,
                Dx = a.Dx - b.Dx,
                Dy = a.Dy - b.Dy,
                Dz = a.Dz - b.Dz
            };

            public static Sample4 operator *(Sample4 a, float4 b) => new()
            {
                V = a.V * b,
                Dx = a.Dx * b,
                Dy = a.Dy * b,
                Dz = a.Dz * b
            };

            public static Sample4 operator *(float4 a, Sample4 b) => b * a;

            public static Sample4 operator /(Sample4 a, float4 b) => new()
            {
                V = a.V / b,
                Dx = a.Dx / b,
                Dy = a.Dy / b,
                Dz = a.Dz / b
            };
        }
    }
}