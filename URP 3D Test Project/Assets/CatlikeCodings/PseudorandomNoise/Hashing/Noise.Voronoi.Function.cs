using Unity.Mathematics;

namespace CatlikeCodings.PseudorandomNoise.Hashing
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-13 06:01:44
    public static partial class Noise
    {
        public interface IVoronoiFunction
        {
            float4 Evaluate(float4x2 minima);
        }

        public struct F1 : IVoronoiFunction
        {
            public float4 Evaluate(float4x2 distances) => distances.c0;
        }

        public struct F2 : IVoronoiFunction
        {
            public float4 Evaluate(float4x2 distances) => distances.c1;
        }

        public struct F2MinusF1 : IVoronoiFunction
        {
            public float4 Evaluate(float4x2 distances) => distances.c1 - distances.c0;
        }
    }
}