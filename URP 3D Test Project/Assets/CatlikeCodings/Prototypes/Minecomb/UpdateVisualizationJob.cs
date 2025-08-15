using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace CatlikeCodings.Prototypes.Minecomb
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-15 21:25:15
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
    public struct UpdateVisualizationJob : IJobFor
    {
        private static readonly ulong[] Bitmaps =
        {
            0b00000_01110_01010_01010_01010_01110_00000, // 0
            0b00000_00100_00110_00100_00100_01110_00000, // 1
            0b00000_01110_01000_01110_00010_01110_00000, // 2
            0b00000_01110_01000_01110_01000_01110_00000, // 3
            0b00000_01010_01010_01110_01000_01000_00000, // 4
            0b00000_01110_00010_01110_01000_01110_00000, // 5
            0b00000_01110_00010_01110_01010_01110_00000, // 6

            0b00000_10001_01010_00100_01010_10001_00000, // mine
            0b00000_00000_00100_01110_00100_00000_00000, // marked sure
            0b11111_11111_11011_10001_11011_11111_11111, // marked mistaken
            0b00000_01110_01010_01000_00100_00000_00100, // marked unsure
            0b00000_00000_00000_00000_00000_00000_00000 // hidden
        };

        private static readonly float3[] Colorations =
        {
            1.00f * float3(1f, 1f, 1f), // 0
            1.00f * float3(0f, 0f, 1f), // 1
            2.00f * float3(0f, 1f, 1f), // 2
            5.00f * float3(0f, 1f, 0f), // 3
            10.0f * float3(1f, 1f, 0f), // 4
            20.0f * float3(1f, 0f, 0f), // 5
            20.0f * float3(1f, 0f, 1f), // 6

            30.0f * float3(1f, 0f, 1f), // mine
            1.00f * float3(1f, 0f, 0f), // marked sure
            50.0f * float3(1f, 0f, 1f), // marked mistaken
            0.25f * float3(1f, 1f, 1f), // marked unsure
            0.00f * float3(0f, 0f, 0f) // hidden
        };

        private enum Symbol
        {
            Mine = 7,
            MarkedSure,
            MarkedMistaken,
            MarkedUnsure,
            Hidden
        }

        private static int GetSymbolIndex(CellState state) =>
            state.Is(CellState.Revealed)
                ? state.Is(CellState.Mine)
                    ? (int)Symbol.Mine
                    : state.Is(CellState.MarkedSure)
                        ? (int)Symbol.MarkedMistaken
                        : (int)state.Without(CellState.Revealed)
                : state.Is(CellState.MarkedSure)
                    ? (int)Symbol.MarkedSure
                    : state.Is(CellState.MarkedUnsure)
                        ? (int)Symbol.MarkedUnsure
                        : (int)Symbol.Hidden;

        [NativeDisableParallelForRestriction] public NativeArray<float3> Positions, Colors, Ripples;
        public int RippleCount;
        [ReadOnly] public Grid Grid;

        public void Execute(int i)
        {
            var blockOffset = i * GridVisualization.BlocksPerCell;
            var symbolIndex = GetSymbolIndex(Grid[i]);
            var bitmap = Bitmaps[symbolIndex];
            var coloration = Colorations[symbolIndex];
            for (var bi = 0; bi < GridVisualization.BlocksPerCell; bi++)
            {
                var altered = (bitmap & ((ulong)1 << bi)) != 0;
                var position = Positions[blockOffset + bi];
                var ripples = AccumulateRipples(position);
                position.y = (altered ? 0.5f : 0f) - 0.5f * ripples;
                Positions[blockOffset + bi] = position;
                Colors[blockOffset + bi] = (altered ? coloration : 0.5f) * (1f - 0.05f * ripples);
            }
        }

        private float AccumulateRipples(float3 position)
        {
            var sum = 0f;
            for (var r = 0; r < RippleCount; r++)
            {
                var ripple = Ripples[r];
                var d = 50f * ripple.z - distance(position.xz, ripple.xy);
                if (d is > 0 and < 10f)
                {
                    sum += (1f - cos(d * 2f * PI / 10f)) * (1f - ripple.z * ripple.z);
                }
            }

            return sum;
        }
    }
}