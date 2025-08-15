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
            Sample4 Evaluate(VoronoiData data);
        }

        public struct F1 : IVoronoiFunction
        {
            public Sample4 Evaluate(VoronoiData data) => data.A;
        }

        public struct F2 : IVoronoiFunction
        {
            public Sample4 Evaluate(VoronoiData data) => data.B;
        }

        public struct F2MinusF1 : IVoronoiFunction
        {
            public Sample4 Evaluate(VoronoiData data) => data.B - data.A;
        }
    }
}