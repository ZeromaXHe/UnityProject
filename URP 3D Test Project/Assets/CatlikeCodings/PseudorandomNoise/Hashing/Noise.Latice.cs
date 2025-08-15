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
            public Sample4 GetNoise4(float4x3 positions, SmallXxHash4 hash, int frequency)
            {
                var l = default(TL);
                var x = l.GetLatticeSpan4(positions.c0, frequency);
                var g = default(TG);
                Sample4 a = g.Evaluate(hash.Eat(x.P0), x.G0),
                    b = g.Evaluate(hash.Eat(x.P1), x.G1);
                return g.EvaluateCombined(new Sample4
                {
                    V = lerp(a.V, b.V, x.T),
                    Dx = frequency * (lerp(a.Dx, b.Dx, x.T) + (b.V - a.V) * x.Dt)
                });
            }
        }

        public struct Lattice2D<TL, TG> : INoise where TL : struct, ILattice where TG : struct, IGradient
        {
            public Sample4 GetNoise4(float4x3 positions, SmallXxHash4 hash, int frequency)
            {
                var l = default(TL);
                var x = l.GetLatticeSpan4(positions.c0, frequency);
                var z = l.GetLatticeSpan4(positions.c2, frequency);
                var h0 = hash.Eat(x.P0);
                var h1 = hash.Eat(x.P1);
                var g = default(TG);
                Sample4
                    a = g.Evaluate(h0.Eat(z.P0), x.G0, z.G0),
                    b = g.Evaluate(h0.Eat(z.P1), x.G0, z.G1),
                    c = g.Evaluate(h1.Eat(z.P0), x.G1, z.G0),
                    d = g.Evaluate(h1.Eat(z.P1), x.G1, z.G1);

                return g.EvaluateCombined(new Sample4
                {
                    V = lerp(lerp(a.V, b.V, z.T), lerp(c.V, d.V, z.T), x.T),
                    Dx = frequency * (
                        lerp(lerp(a.Dx, b.Dx, z.T), lerp(c.Dx, d.Dx, z.T), x.T) +
                        (lerp(c.V, d.V, z.T) - lerp(a.V, b.V, z.T)) * x.Dt
                    ),
                    Dz = frequency * lerp(lerp(a.Dz, b.Dz, z.T) + (b.V - a.V) * z.Dt,
                        lerp(c.Dz, d.Dz, z.T) + (d.V - c.V) * z.Dt, x.T)
                });
            }
        }

        public struct Lattice3D<TL, TG> : INoise where TL : struct, ILattice where TG : struct, IGradient
        {
            public Sample4 GetNoise4(float4x3 positions, SmallXxHash4 hash, int frequency)
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

                var gradient = default(TG);
                Sample4
                    a = gradient.Evaluate(h00.Eat(z.P0), x.G0, y.G0, z.G0),
                    b = gradient.Evaluate(h00.Eat(z.P1), x.G0, y.G0, z.G1),
                    c = gradient.Evaluate(h01.Eat(z.P0), x.G0, y.G1, z.G0),
                    d = gradient.Evaluate(h01.Eat(z.P1), x.G0, y.G1, z.G1),
                    e = gradient.Evaluate(h10.Eat(z.P0), x.G1, y.G0, z.G0),
                    f = gradient.Evaluate(h10.Eat(z.P1), x.G1, y.G0, z.G1),
                    g = gradient.Evaluate(h11.Eat(z.P0), x.G1, y.G1, z.G0),
                    h = gradient.Evaluate(h11.Eat(z.P1), x.G1, y.G1, z.G1);

                return gradient.EvaluateCombined(new Sample4
                {
                    V = lerp(
                        lerp(lerp(a.V, b.V, z.T), lerp(c.V, d.V, z.T), y.T),
                        lerp(lerp(e.V, f.V, z.T), lerp(g.V, h.V, z.T), y.T),
                        x.T
                    ),
                    Dx = frequency * (
                        lerp(
                            lerp(lerp(a.Dx, b.Dx, z.T), lerp(c.Dx, d.Dx, z.T), y.T),
                            lerp(lerp(e.Dx, f.Dx, z.T), lerp(g.Dx, h.Dx, z.T), y.T),
                            x.T
                        ) + (
                            lerp(lerp(e.V, f.V, z.T), lerp(g.V, h.V, z.T), y.T) -
                            lerp(lerp(a.V, b.V, z.T), lerp(c.V, d.V, z.T), y.T)
                        ) * x.Dt
                    ),
                    Dy = frequency * lerp(
                        lerp(lerp(a.Dy, b.Dy, z.T), lerp(c.Dy, d.Dy, z.T), y.T) +
                        (lerp(c.V, d.V, z.T) - lerp(a.V, b.V, z.T)) * y.Dt,
                        lerp(lerp(e.Dy, f.Dy, z.T), lerp(g.Dy, h.Dy, z.T), y.T) +
                        (lerp(g.V, h.V, z.T) - lerp(e.V, f.V, z.T)) * y.Dt,
                        x.T
                    ),
                    Dz = frequency * lerp(
                        lerp(
                            lerp(a.Dz, b.Dz, z.T) + (b.V - a.V) * z.Dt,
                            lerp(c.Dz, d.Dz, z.T) + (d.V - c.V) * z.Dt,
                            y.T
                        ),
                        lerp(
                            lerp(e.Dz, f.Dz, z.T) + (f.V - e.V) * z.Dt,
                            lerp(g.Dz, h.Dz, z.T) + (h.V - g.V) * z.Dt,
                            y.T
                        ),
                        x.T
                    )
                });
            }
        }

        public struct LatticeSpan4
        {
            public int4 P0, P1;
            public float4 G0, G1;
            public float4 T, Dt;
        }

        public interface ILattice
        {
            LatticeSpan4 GetLatticeSpan4(float4 coordinates, int frequency);
            int4 ValidateSingleStep(int4 points, int frequency);
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
                var t = coordinates - points;
                span.T = t * t * t * (t * (t * 6f - 15f) + 10f);
                span.Dt = t * t * (t * (t * 30f - 60f) + 30f);
                return span;
            }

            public int4 ValidateSingleStep(int4 points, int frequency) => points;
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

                var t = coordinates - points;
                span.T = t * t * t * (t * (t * 6f - 15f) + 10f);
                span.Dt = t * t * (t * (t * 30f - 60f) + 30f);
                return span;
            }

            public int4 ValidateSingleStep(int4 points, int frequency) =>
                select(select(points, 0, points == frequency), frequency - 1, points == -1);
        }
    }
}