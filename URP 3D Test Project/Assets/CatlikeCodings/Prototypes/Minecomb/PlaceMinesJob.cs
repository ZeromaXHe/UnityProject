using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace CatlikeCodings.Prototypes.Minecomb
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-15 21:47:15
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
    public struct PlaceMinesJob : IJob
    {
        public Grid Grid;
        public int Mines, Seed;

        public void Execute()
        {
            Grid.RevealedCellCount = 0;
            var candidateCount = Grid.CellCount;
            var candidates =
                new NativeArray<int>(candidateCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            for (var i = 0; i < Grid.CellCount; i++)
            {
                Grid[i] = CellState.Zero;
                candidates[i] = i;
            }

            var random = new Random((uint)Seed);
            for (var m = 0; m < Mines; m++)
            {
                var candidateIndex = random.NextInt(candidateCount--);
                SetMine(candidates[candidateIndex]);
                candidates[candidateIndex] = candidates[candidateCount];
            }
        }

        private void SetMine(int i)
        {
            Grid[i] = Grid[i].With(CellState.Mine);
            Grid.GetRowColumn(i, out var r, out var c);
            Increment(r - 1, c);
            Increment(r + 1, c);
            Increment(r, c - 1);
            Increment(r, c + 1);
            
            var rowOffset = (c & 1) == 0 ? 1 : -1;
            Increment(r + rowOffset, c - 1);
            Increment(r + rowOffset, c + 1);
        }

        private void Increment (int r, int c)
        {
            if (Grid.TryGetCellIndex(r, c, out var i))
            {
                Grid[i] += 1;
            }
        }
    }
}