using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace CatlikeCodings.PseudorandomNoise.Hashing
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-12 20:19:59
    public static partial class Noise
    {
        public struct Lattice1D<TL, TG> : INoise where TL : struct, ILattice where TG : struct, IGradient
        {
            public float4 GetNoise4(float4x3 positions, SmallXxHash4 hash, int frequency)
            {
                var l = default(TL);
                var x = l.GetLatticeSpan4(positions.c0, frequency);
                var g = default(TG);
                return g.EvaluateAfterInterpolation(lerp(
                    g.Evaluate(hash.Eat(x.P0), x.G0), g.Evaluate(hash.Eat(x.P1), x.G1), x.T));
            }
        }

        public struct Lattice2D<TL, TG> : INoise where TL : struct, ILattice where TG : struct, IGradient
        {
            public float4 GetNoise4(float4x3 positions, SmallXxHash4 hash, int frequency)
            {
                var l = default(TL);
                var x = l.GetLatticeSpan4(positions.c0, frequency);
                var z = l.GetLatticeSpan4(positions.c2, frequency);
                var h0 = hash.Eat(x.P0);
                var h1 = hash.Eat(x.P1);
                var g = default(TG);
                return g.EvaluateAfterInterpolation(lerp(
                    lerp(g.Evaluate(h0.Eat(z.P0), x.G0, z.G0), g.Evaluate(h0.Eat(z.P1), x.G0, z.G1), z.T),
                    lerp(g.Evaluate(h1.Eat(z.P0), x.G1, z.G0), g.Evaluate(h1.Eat(z.P1), x.G1, z.G1), z.T),
                    x.T
                ));
            }
        }

        public struct Lattice3D<TL, TG> : INoise where TL : struct, ILattice where TG : struct, IGradient
        {
            public float4 GetNoise4(float4x3 positions, SmallXxHash4 hash, int frequency)
            {
                var l = default(TL);
                LatticeSpan4
                    x = l.GetLatticeSpan4(positions.c0, frequency),
                    y = l.GetLatticeSpan4(positions.c1, frequency),
                    z = l.GetLatticeSpan4(positions.c2, frequency);

                SmallXxHash4
                    h0 = hash.Eat(x.P0),
                    h1 = hash.Eat(x.P1),
                    h00 = h0.Eat(y.P0),
                    h01 = h0.Eat(y.P1),
                    h10 = h1.Eat(y.P0),
                    h11 = h1.Eat(y.P1);

                var g = default(TG);
                return g.EvaluateAfterInterpolation(lerp(
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
                ));
            }
        }

        public struct LatticeSpan4
        {
            public int4 P0, P1;
            public float4 G0, G1;
            public float4 T;
        }

        public interface ILattice
        {
            LatticeSpan4 GetLatticeSpan4(float4 coordinates, int frequency);
        }

        public struct LatticeNormal : ILattice
        {
            public LatticeSpan4 GetLatticeSpan4(float4 coordinates, int frequency)
            {
                coordinates *= frequency;
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

        public struct LatticeTiling : ILattice
        {
            public LatticeSpan4 GetLatticeSpan4(float4 coordinates, int frequency)
            {
                coordinates *= frequency;
                var points = floor(coordinates);
                LatticeSpan4 span;
                span.P0 = (int4)points;
                span.G0 = coordinates - span.P0;
                span.G1 = span.G0 - 1f;

                span.P0 -= (int4)ceil(points / frequency) * frequency;
                span.P0 = select(span.P0, span.P0 + frequency, span.P0 < 0);
                span.P1 = span.P0 + 1;
                span.P1 = select(span.P1, 0, span.P1 == frequency);

                span.T = coordinates - points;
                span.T = span.T * span.T * span.T * (span.T * (span.T * 6f - 15f) + 10f);
                return span;
            }
        }
    }
}