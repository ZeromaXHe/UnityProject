using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace CatlikeCodings.Prototypes.Minecomb
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-15 22:02:15
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
    public struct RevealMinesAndMistakesJob : IJobFor
    {
        public Grid Grid;

        public void Execute (int i) => Grid[i] = Grid[i].With(
            Grid[i].Is(CellState.MarkedSureOrMine) ? CellState.Revealed : CellState.Zero
        );
    }
}