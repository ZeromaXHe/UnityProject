using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace CatlikeCodings.PseudorandomNoise.Hashing
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-12 20:19:59
    public static partial class Noise
    {
        public struct Lattice1D : INoise
        {
            public float4 GetNoise4(float4x3 positions, SmallXxHash4 hash)
            {
                var x = GetLatticeSpan4(positions.c0);
                return lerp(hash.Eat(x.P0).Floats01A, hash.Eat(x.P1).Floats01A, x.T) * 2f - 1f;
            }
        }

        public struct Lattice2D : INoise
        {
            public float4 GetNoise4(float4x3 positions, SmallXxHash4 hash)
            {
                var x = GetLatticeSpan4(positions.c0);
                var z = GetLatticeSpan4(positions.c2);
                var h0 = hash.Eat(x.P0);
                var h1 = hash.Eat(x.P1);
                return lerp(
                    lerp(h0.Eat(z.P0).Floats01A, h0.Eat(z.P1).Floats01A, z.T),
                    lerp(h1.Eat(z.P0).Floats01A, h1.Eat(z.P1).Floats01A, z.T),
                    x.T
                ) * 2f - 1f;
            }
        }

        public struct Lattice3D : INoise
        {
            public float4 GetNoise4(float4x3 positions, SmallXxHash4 hash)
            {
                LatticeSpan4
                    x = GetLatticeSpan4(positions.c0),
                    y = GetLatticeSpan4(positions.c1),
                    z = GetLatticeSpan4(positions.c2);

                SmallXxHash4
                    h0 = hash.Eat(x.P0),
                    h1 = hash.Eat(x.P1),
                    h00 = h0.Eat(y.P0),
                    h01 = h0.Eat(y.P1),
                    h10 = h1.Eat(y.P0),
                    h11 = h1.Eat(y.P1);

                return lerp(
                    lerp(
                        lerp(h00.Eat(z.P0).Floats01A, h00.Eat(z.P1).Floats01A, z.T),
                        lerp(h01.Eat(z.P0).Floats01A, h01.Eat(z.P1).Floats01A, z.T),
                        y.T
                    ),
                    lerp(
                        lerp(h10.Eat(z.P0).Floats01A, h10.Eat(z.P1).Floats01A, z.T),
                        lerp(h11.Eat(z.P0).Floats01A, h11.Eat(z.P1).Floats01A, z.T),
                        y.T
                    ),
                    x.T
                ) * 2f - 1f;
            }
        }

        private struct LatticeSpan4
        {
            public int4 P0, P1;
            public float4 T;
        }

        static LatticeSpan4 GetLatticeSpan4(float4 coordinates)
        {
            var points = floor(coordinates);
            LatticeSpan4 span;
            span.P0 = (int4)points;
            span.P1 = span.P0 + 1;
            span.T = coordinates - points;
            span.T = span.T * span.T * span.T * (span.T * (span.T * 6f - 15f) + 10f);
            return span;
        }
    }
}