using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace CatlikeCodings.PseudorandomNoise.Hashing
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-13 06:10:50
    public static partial class Noise
    {
        public interface IVoronoiDistance
        {
            Sample4 GetDistance(float4 x);
            Sample4 GetDistance(float4 x, float4 y);
            Sample4 GetDistance(float4 x, float4 y, float4 z);
            VoronoiData Finalize1D(VoronoiData data);
            VoronoiData Finalize2D(VoronoiData data);
            VoronoiData Finalize3D(VoronoiData data);
            VoronoiData UpdateVoronoiData(VoronoiData data, Sample4 sample);
            VoronoiData InitialData { get; }
        }

        public struct Worley : IVoronoiDistance
        {
            public Sample4 GetDistance(float4 x) => new()
            {
                V = abs(x),
                Dx = select(-1f, 1f, x < 0f)
            };

            public Sample4 GetDistance(float4 x, float4 y) => GetDistance(x, 0f, y);

            public Sample4 GetDistance(float4 x, float4 y, float4 z) => new()
            {
                V = x * x + y * y + z * z,
                Dx = x,
                Dy = y,
                Dz = z
            };

            public VoronoiData Finalize1D(VoronoiData data) => data;
            public VoronoiData Finalize2D(VoronoiData data) => Finalize3D(data);

            public VoronoiData Finalize3D(VoronoiData data)
            {
                var keepA = data.A.V < 1f;
                data.A.V = select(1f, sqrt(data.A.V), keepA);
                data.A.Dx = select(0f, -data.A.Dx / data.A.V, keepA);
                data.A.Dy = select(0f, -data.A.Dy / data.A.V, keepA);
                data.A.Dz = select(0f, -data.A.Dz / data.A.V, keepA);

                var keepB = data.B.V < 1f;
                data.B.V = select(1f, sqrt(data.B.V), keepB);
                data.B.Dx = select(0f, -data.B.Dx / data.B.V, keepB);
                data.B.Dy = select(0f, -data.B.Dy / data.B.V, keepB);
                data.B.Dz = select(0f, -data.B.Dz / data.B.V, keepB);
                return data;
            }

            public VoronoiData UpdateVoronoiData(VoronoiData data, Sample4 sample)
            {
                bool4 newMinimum = sample.V < data.A.V;
                data.B = Sample4.Select(
                    Sample4.Select(data.B, sample, sample.V < data.B.V),
                    data.A,
                    newMinimum
                );
                data.A = Sample4.Select(data.A, sample, newMinimum);
                return data;
            }

            public VoronoiData InitialData => new VoronoiData
            {
                A = new Sample4 { V = 2f },
                B = new Sample4 { V = 2f }
            };
        }

        public struct SmoothWorley : IVoronoiDistance
        {
            private const float SmoothLse = 10f, SmoothPoly = 0.25f;
            public Sample4 GetDistance(float4 x) => default(Worley).GetDistance(x);
            public Sample4 GetDistance(float4 x, float4 y) => GetDistance(x, 0f, y);

            public Sample4 GetDistance(float4 x, float4 y, float4 z)
            {
                var v = sqrt(x * x + y * y + z * z);
                return new Sample4
                {
                    V = v,
                    Dx = x / -v,
                    Dy = y / -v,
                    Dz = y / -v
                };
            }

            public VoronoiData Finalize1D(VoronoiData data)
            {
                data.A.Dx /= data.A.V;
                data.A.V = log(data.A.V) / -SmoothLse;
                data.A = Sample4.Select(default, data.A.Smoothstep, data.A.V > 0f);
                data.B = Sample4.Select(default, data.B.Smoothstep, data.B.V > 0f);
                return data;
            }

            public VoronoiData Finalize2D(VoronoiData data) => Finalize3D(data);

            public VoronoiData Finalize3D(VoronoiData data)
            {
                data.A.Dx /= data.A.V;
                data.A.Dy /= data.A.V;
                data.A.Dz /= data.A.V;
                data.A.V = log(data.A.V) / -SmoothLse;
                data.A = Sample4.Select(default, data.A.Smoothstep, data.A.V > 0f & data.A.V < 1f);
                data.B = Sample4.Select(default, data.B.Smoothstep, data.B.V > 0f & data.B.V < 1f);
                return data;
            }

            public VoronoiData UpdateVoronoiData(VoronoiData data, Sample4 sample)
            {
                var e = exp(-SmoothLse * sample.V);
                data.A.V += e;
                data.A.Dx += e * sample.Dx;
                data.A.Dy += e * sample.Dy;
                data.A.Dz += e * sample.Dz;
                var h = 1f - abs(data.B.V - sample.V) / SmoothPoly;
                float4
                    hdx = data.B.Dx - sample.Dx,
                    hdy = data.B.Dy - sample.Dy,
                    hdz = data.B.Dz - sample.Dz;
                var ds = data.B.V - sample.V < 0f;
                hdx = select(-hdx, hdx, ds) * 0.5f * h;
                hdy = select(-hdy, hdy, ds) * 0.5f * h;
                hdz = select(-hdz, hdz, ds) * 0.5f * h;
                var smooth = h > 0f;
                h = 0.25f * SmoothPoly * h * h;
                data.B = Sample4.Select(data.B, sample, sample.V < data.B.V);
                data.B.V -= select(0f, h, smooth);
                data.B.Dx -= select(0f, hdx, smooth);
                data.B.Dy -= select(0f, hdy, smooth);
                data.B.Dz -= select(0f, hdz, smooth);
                return data;
            }

            public VoronoiData InitialData => new()
            {
                B = new Sample4 { V = 2f }
            };
        }

        public struct Chebyshev : IVoronoiDistance
        {
            public Sample4 GetDistance(float4 x) => default(Worley).GetDistance(x);

            public Sample4 GetDistance(float4 x, float4 y)
            {
                var keepX = abs(x) > abs(y);
                return new Sample4
                {
                    V = select(abs(y), abs(x), keepX),
                    Dx = select(0f, select(-1f, 1f, x < 0f), keepX),
                    Dz = select(select(-1f, 1f, y < 0f), 0f, keepX)
                };
            }

            public Sample4 GetDistance(float4 x, float4 y, float4 z)
            {
                var keepX = abs(x) > abs(y) & abs(x) > abs(z);
                var keepY = abs(y) > abs(z);
                return new Sample4
                {
                    V = select(select(abs(z), abs(y), keepY), abs(x), keepX),
                    Dx = select(0f, select(-1f, 1f, x < 0f), keepX),
                    Dy = select(select(0f, select(-1f, 1f, y < 0f), keepY), 0f, keepX),
                    Dz = select(select(select(-1f, 1f, z < 0f), 0f, keepY), 0f, keepX)
                };
            }

            public VoronoiData Finalize1D(VoronoiData data) => data;
            public VoronoiData Finalize2D(VoronoiData data) => data;
            public VoronoiData Finalize3D(VoronoiData data) => data;

            public VoronoiData UpdateVoronoiData(VoronoiData data, Sample4 sample) =>
                default(Worley).UpdateVoronoiData(data, sample);

            public VoronoiData InitialData => default(Worley).InitialData;
        }
    }
}