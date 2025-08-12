using Unity.Mathematics;

namespace CatlikeCodings.PseudorandomNoise.Hashing
{
    /// Copyright (C) 2025 Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-08 11:02:08
    public readonly struct SmallXxHash
    {
        private const uint PrimeA = 0b10011110001101110111100110110001;
        private const uint PrimeB = 0b10000101111010111100101001110111;
        private const uint PrimeC = 0b11000010101100101010111000111101;
        private const uint PrimeD = 0b00100111110101001110101100101111;
        private const uint PrimeE = 0b00010110010101100110011110110001;
        public readonly uint Accumulator;

        public SmallXxHash(uint accumulator)
        {
            Accumulator = accumulator;
        }

        public static implicit operator SmallXxHash(uint accumulator) => new(accumulator);

        public static implicit operator uint(SmallXxHash hash)
        {
            var avalanche = hash.Accumulator;
            avalanche ^= avalanche >> 15;
            avalanche *= PrimeB;
            avalanche ^= avalanche >> 13;
            avalanche *= PrimeC;
            avalanche ^= avalanche >> 16;
            return avalanche;
        }

        public static SmallXxHash Seed(int seed) => (uint)seed + PrimeE;
        public SmallXxHash Eat(int data) => RotateLeft(Accumulator + (uint)data * PrimeC, 17) * PrimeD;
        public SmallXxHash Eat(byte data) => RotateLeft(Accumulator + data * PrimeE, 11) * PrimeA;
        private static uint RotateLeft(uint data, int steps) => (data << steps) | (data >> 32 - steps);
    }

    public readonly struct SmallXxHash4
    {
        private const uint PrimeB = 0b10000101111010111100101001110111;
        private const uint PrimeC = 0b11000010101100101010111000111101;
        private const uint PrimeD = 0b00100111110101001110101100101111;
        private const uint PrimeE = 0b00010110010101100110011110110001;
        private readonly uint4 _accumulator;

        public SmallXxHash4(uint4 accumulator)
        {
            _accumulator = accumulator;
        }

        public static implicit operator SmallXxHash4(uint4 accumulator) => new(accumulator);
        public static implicit operator SmallXxHash4(SmallXxHash hash) => new(hash.Accumulator);

        public static implicit operator uint4(SmallXxHash4 hash)
        {
            var avalanche = hash._accumulator;
            avalanche ^= avalanche >> 15;
            avalanche *= PrimeB;
            avalanche ^= avalanche >> 13;
            avalanche *= PrimeC;
            avalanche ^= avalanche >> 16;
            return avalanche;
        }

        public static SmallXxHash4 operator +(SmallXxHash4 h, int v) => h._accumulator + (uint)v;
        public static SmallXxHash4 Seed(int4 seed) => (uint4)seed + PrimeE;
        public SmallXxHash4 Eat(int4 data) => RotateLeft(_accumulator + (uint4)data * PrimeC, 17) * PrimeD;
        private static uint4 RotateLeft(uint4 data, int steps) => (data << steps) | (data >> 32 - steps);
        public uint4 BytesA => (uint4)this & 255;
        public uint4 BytesB => ((uint4)this >> 8) & 255;
        public uint4 BytesC => ((uint4)this >> 16) & 255;
        public uint4 BytesD => (uint4)this >> 24;
        public float4 Floats01A => (float4)BytesA * (1f / 255f);
        public float4 Floats01B => (float4)BytesB * (1f / 255f);
        public float4 Floats01C => (float4)BytesC * (1f / 255f);
        public float4 Floats01D => (float4)BytesD * (1f / 255f);

        public uint4 GetBits(int count, int shift) =>
            ((uint4)this >> shift) & (uint)((1 << count) - 1);

        public float4 GetBitsAsFloats01(int count, int shift) =>
            (float4)GetBits(count, shift) * (1f / ((1 << count) - 1));

        public static SmallXxHash4 Select(SmallXxHash4 a, SmallXxHash4 b, bool4 c) =>
            math.select(a._accumulator, b._accumulator, c);
    }
}