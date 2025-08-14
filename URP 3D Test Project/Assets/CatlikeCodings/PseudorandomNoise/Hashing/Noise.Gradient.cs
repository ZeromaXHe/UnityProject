using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace CatlikeCodings.PseudorandomNoise.Hashing
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-12 20:43:19
    public static partial class Noise
    {
        public interface IGradient
        {
            Sample4 Evaluate(SmallXxHash4 hash, float4 x);
            Sample4 Evaluate(SmallXxHash4 hash, float4 x, float4 y);
            Sample4 Evaluate(SmallXxHash4 hash, float4 x, float4 y, float4 z);
            Sample4 EvaluateCombined(Sample4 value);
        }

        public struct Value : IGradient
        {
            public Sample4 Evaluate(SmallXxHash4 hash, float4 x) => hash.Floats01A * 2f - 1f;
            public Sample4 Evaluate(SmallXxHash4 hash, float4 x, float4 y) => hash.Floats01A * 2f - 1f;
            public Sample4 Evaluate(SmallXxHash4 hash, float4 x, float4 y, float4 z) => hash.Floats01A * 2f - 1f;
            public Sample4 EvaluateCombined(Sample4 value) => value;
        }

        public struct Perlin : IGradient
        {
            public Sample4 Evaluate(SmallXxHash4 hash, float4 x) => BaseGradients.Line(hash, x);

            public Sample4 Evaluate(SmallXxHash4 hash, float4 x, float4 y) =>
                BaseGradients.Square(hash, x, y) * (2f / 0.53528f);

            public Sample4 Evaluate(SmallXxHash4 hash, float4 x, float4 y, float4 z) =>
                BaseGradients.Octahedron(hash, x, y, z) * (1f / 0.56290f);

            public Sample4 EvaluateCombined(Sample4 value) => value;
        }

        public struct Simplex : IGradient
        {
            public Sample4 Evaluate(SmallXxHash4 hash, float4 x) =>
                BaseGradients.Line(hash, x) * (32f / 27f);

            public Sample4 Evaluate(SmallXxHash4 hash, float4 x, float4 y) =>
                BaseGradients.Circle(hash, x, y) * (5.832f / sqrt(2f));

            public Sample4 Evaluate(SmallXxHash4 hash, float4 x, float4 y, float4 z) =>
                BaseGradients.Sphere(hash, x, y, z) * (1024f / (125f * sqrt(3f)));

            public Sample4 EvaluateCombined(Sample4 value) => value;
        }

        public static class BaseGradients
        {
            public static Sample4 Line(SmallXxHash4 hash, float4 x)
            {
                var l = (1f + hash.Floats01A) * select(-1f, 1f, ((uint4)hash & 1 << 8) == 0);
                return new Sample4
                {
                    V = l * x,
                    Dx = l
                };
            }

            private static float4x2 SquareVectors(SmallXxHash4 hash)
            {
                float4x2 v;
                v.c0 = hash.Floats01A * 2f - 1f;
                v.c1 = 0.5f - abs(v.c0);
                v.c0 -= floor(v.c0 + 0.5f);
                return v;
            }

            private static float4x3 OctahedronVectors(SmallXxHash4 hash)
            {
                float4x3 g;
                g.c0 = hash.Floats01A * 2f - 1f;
                g.c1 = hash.Floats01D * 2f - 1f;
                g.c2 = 1f - abs(g.c0) - abs(g.c1);
                var offset = max(-g.c2, 0f);
                g.c0 += select(-offset, offset, g.c0 < 0f);
                g.c1 += select(-offset, offset, g.c1 < 0f);
                return g;
            }

            public static float4 Square(SmallXxHash4 hash, float4 x, float4 y)
            {
                var v = SquareVectors(hash);
                return v.c0 * x + v.c1 * y;
            }

            public static Sample4 Circle(SmallXxHash4 hash, float4 x, float4 y)
            {
                var v = SquareVectors(hash);
                return new Sample4
                {
                    V = v.c0 * x + v.c1 * y,
                    Dx = v.c0,
                    Dz = v.c1
                } * rsqrt(v.c0 * v.c0 + v.c1 * v.c1);
            }

            public static float4 Octahedron(
                SmallXxHash4 hash, float4 x, float4 y, float4 z
            )
            {
                var v = OctahedronVectors(hash);
                return v.c0 * x + v.c1 * y + v.c2 * z;
            }

            public static Sample4 Sphere(SmallXxHash4 hash, float4 x, float4 y, float4 z)
            {
                var v = OctahedronVectors(hash);
                return
                    new Sample4
                    {
                        V = v.c0 * x + v.c1 * y + v.c2 * z,
                        Dx = v.c0,
                        Dy = v.c1,
                        Dz = v.c2
                    } * rsqrt(v.c0 * v.c0 + v.c1 * v.c1 + v.c2 * v.c2);
            }
        }

        public struct Turbulence<TG> : IGradient where TG : struct, IGradient
        {
            public Sample4 Evaluate(SmallXxHash4 hash, float4 x) =>
                default(TG).Evaluate(hash, x);

            public Sample4 Evaluate(SmallXxHash4 hash, float4 x, float4 y) =>
                default(TG).Evaluate(hash, x, y);

            public Sample4 Evaluate(SmallXxHash4 hash, float4 x, float4 y, float4 z) =>
                default(TG).Evaluate(hash, x, y, z);

            public Sample4 EvaluateCombined(Sample4 value)
            {
                var s = default(TG).EvaluateCombined(value);
                s.Dx = select(-s.Dx, s.Dx, s.V >= 0f);
                s.Dy = select(-s.Dy, s.Dy, s.V >= 0f);
                s.Dz = select(-s.Dz, s.Dz, s.V >= 0f);
                s.V = abs(s.V);
                return s;
            }
        }
        
        public struct Smoothstep<G> : IGradient where G : struct, IGradient {

            public Sample4 Evaluate (SmallXxHash4 hash, float4 x) =>
                default(G).Evaluate(hash, x);

            public Sample4 Evaluate (SmallXxHash4 hash, float4 x, float4 y) =>
                default(G).Evaluate(hash, x, y);

            public Sample4 Evaluate (SmallXxHash4 hash, float4 x, float4 y, float4 z) =>
                default(G).Evaluate(hash, x, y, z);

            public Sample4 EvaluateCombined (Sample4 value) {
                var s = default(G).EvaluateCombined(value);
                var d = 6f * s.V * (1f - s.V);
                s.Dx *= d;
                s.Dy *= d;
                s.Dz *= d;
                s.V *= s.V * (3f - 2f * s.V);
                return s;
            }
        }
    }
}