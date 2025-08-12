using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace CatlikeCodings.PseudorandomNoise.Hashing
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-12 20:19:59
    public static partial class Noise
    {
        public struct Lattice1D<TG> : INoise where TG : struct, IGradient
        {
            public float4 GetNoise4(float4x3 positions, SmallXxHash4 hash)
            {
                var x = GetLatticeSpan4(positions.c0);
                var g = default(TG);
                return lerp(g.Evaluate(hash.Eat(x.P0), x.G0), g.Evaluate(hash.Eat(x.P1), x.G1), x.T);
            }
        }

        public struct Lattice2D<TG> : INoise where TG : struct, IGradient
        {
            public float4 GetNoise4(float4x3 positions, SmallXxHash4 hash)
            {
                var x = GetLatticeSpan4(positions.c0);
                var z = GetLatticeSpan4(positions.c2);
                var h0 = hash.Eat(x.P0);
                var h1 = hash.Eat(x.P1);
                var g = default(TG);
                return lerp(
                    lerp(g.Evaluate(h0.Eat(z.P0), x.G0, z.G0), g.Evaluate(h0.Eat(z.P1), x.G0, z.G1), z.T),
                    lerp(g.Evaluate(h1.Eat(z.P0), x.G1, z.G0), g.Evaluate(h1.Eat(z.P1), x.G1, z.G1), z.T),
                    x.T
                );
            }
        }

        public struct Lattice3D<TG> : INoise where TG : struct, IGradient
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

                var g = default(TG);
                return lerp(
                    lerp(
                        lerp(
                            g.Evaluate(h00.Eat(z.P0), x.G0, y.G0, z.G0),
                            g.Evaluate(h00.Eat(z.P1), x.G0, y.G0, z.G1),
                            z.T),
                        lerp(
                            g.Evaluate(h01.Eat(z.P0), x.G0, y.G1, z.G0),
                            g.Evaluate(h01.Eat(z.P1), x.G0, y.G1, z.G1),
                            z.T),
                        y.T
                    ),
                    lerp(
                        lerp(
                            g.Evaluate(h10.Eat(z.P0), x.G1, y.G0, z.G0),
                            g.Evaluate(h10.Eat(z.P1), x.G1, y.G0, z.G1),
                            z.T),
                        lerp(
                            g.Evaluate(h11.Eat(z.P0), x.G1, y.G1, z.G0),
                            g.Evaluate(h11.Eat(z.P1), x.G1, y.G1, z.G1),
                            z.T),
                        y.T
                    ),
                    x.T
                );
            }
        }

        private struct LatticeSpan4
        {
            public int4 P0, P1;
            public float4 G0, G1;
            public float4 T;
        }

        private static LatticeSpan4 GetLatticeSpan4(float4 coordinates)
        {
            var points = floor(coordinates);
            LatticeSpan4 span;
            span.P0 = (int4)points;
            span.P1 = span.P0 + 1;
            span.G0 = coordinates - span.P0;
            span.G1 = span.G0 - 1f;
            span.T = coordinates - points;
            span.T = span.T * span.T * span.T * (span.T * (span.T * 6f - 15f) + 10f);
            return span;
        }
    }
}