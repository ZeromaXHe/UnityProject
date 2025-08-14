using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace CatlikeCodings.PseudorandomNoise.Hashing
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-13 05:38:57
    public static partial class Noise
    {
        private static float4x2 UpdateVoronoiMinima(float4x2 minima, float4 distances)
        {
            var newMinimum = distances < minima.c0;
            minima.c1 = select(select(minima.c1, distances, distances < minima.c1), minima.c0, newMinimum);
            minima.c0 = select(minima.c0, distances, newMinimum);
            return minima;
        }

        public struct Voronoi1D<TL, TD, TF> : INoise
            where TL : struct, ILattice
            where TD : struct, IVoronoiDistance
            where TF : struct, IVoronoiFunction
        {
            public Sample4 GetNoise4(float4x3 positions, SmallXxHash4 hash, int frequency)
            {
                var l = default(TL);
                var d = default(TD);
                var x = l.GetLatticeSpan4(positions.c0, frequency);
                float4x2 minima = 2f;
                for (var u = -1; u <= 1; u++)
                {
                    var h = hash.Eat(l.ValidateSingleStep(x.P0 + u, frequency));
                    minima = UpdateVoronoiMinima(minima, d.GetDistance(h.Floats01A + u - x.G0));
                }

                return default(TF).Evaluate(d.Finalize1D(minima));
            }
        }

        public struct Voronoi2D<TL, TD, TF> : INoise
            where TL : struct, ILattice
            where TD : struct, IVoronoiDistance
            where TF : struct, IVoronoiFunction
        {
            public Sample4 GetNoise4(float4x3 positions, SmallXxHash4 hash, int frequency)
            {
                var l = default(TL);
                var d = default(TD);
                LatticeSpan4
                    x = l.GetLatticeSpan4(positions.c0, frequency),
                    z = l.GetLatticeSpan4(positions.c2, frequency);
                float4x2 minima = 2f;
                for (var u = -1; u <= 1; u++)
                {
                    var hx = hash.Eat(l.ValidateSingleStep(x.P0 + u, frequency));
                    var xOffset = u - x.G0;
                    for (var v = -1; v <= 1; v++)
                    {
                        var h = hx.Eat(l.ValidateSingleStep(z.P0 + v, frequency));
                        var zOffset = v - z.G0;
                        minima = UpdateVoronoiMinima(minima, d.GetDistance(
                            h.Floats01A + xOffset, h.Floats01B + zOffset
                        ));
                        minima = UpdateVoronoiMinima(minima, d.GetDistance(
                            h.Floats01C + xOffset, h.Floats01D + zOffset
                        ));
                    }
                }

                return default(TF).Evaluate(d.Finalize2D(minima));
            }
        }

        public struct Voronoi3D<TL, TD, TF> : INoise
            where TL : struct, ILattice
            where TD : struct, IVoronoiDistance
            where TF : struct, IVoronoiFunction
        {
            public Sample4 GetNoise4(float4x3 positions, SmallXxHash4 hash, int frequency)
            {
                var l = default(TL);
                var d = default(TD);
                LatticeSpan4
                    x = l.GetLatticeSpan4(positions.c0, frequency),
                    y = l.GetLatticeSpan4(positions.c1, frequency),
                    z = l.GetLatticeSpan4(positions.c2, frequency);
                float4x2 minima = 2f;
                for (var u = -1; u <= 1; u++)
                {
                    var hx = hash.Eat(l.ValidateSingleStep(x.P0 + u, frequency));
                    var xOffset = u - x.G0;
                    for (var v = -1; v <= 1; v++)
                    {
                        var hy = hx.Eat(l.ValidateSingleStep(y.P0 + v, frequency));
                        var yOffset = v - y.G0;
                        for (var w = -1; w <= 1; w++)
                        {
                            var h = hy.Eat(l.ValidateSingleStep(z.P0 + w, frequency));
                            var zOffset = w - z.G0;
                            minima = UpdateVoronoiMinima(minima, d.GetDistance(
                                h.GetBitsAsFloats01(5, 0) + xOffset,
                                h.GetBitsAsFloats01(5, 5) + yOffset,
                                h.GetBitsAsFloats01(5, 10) + zOffset
                            ));
                            minima = UpdateVoronoiMinima(minima, d.GetDistance(
                                h.GetBitsAsFloats01(5, 15) + xOffset,
                                h.GetBitsAsFloats01(5, 20) + yOffset,
                                h.GetBitsAsFloats01(5, 25) + zOffset
                            ));
                        }
                    }
                }

                return default(TF).Evaluate(d.Finalize3D(minima));
            }
        }
    }
}