using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace CatlikeCodings.Prototypes.Minecomb
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-15 21:57:15
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
    public struct RevealRegionJob : IJob
    {
        public Grid Grid;
        public int2 StartRowColumn;

        private int _stackSize;

        public void Execute()
        {
            var stack = new NativeArray<int2>(Grid.CellCount, Allocator.Temp);
            _stackSize = 0;
            PushIfNeeded(stack, StartRowColumn);
            while (_stackSize > 0)
            {
                var rc = stack[--_stackSize];
                PushIfNeeded(stack, rc - int2(1, 0));
                PushIfNeeded(stack, rc + int2(1, 0));
                PushIfNeeded(stack, rc - int2(0, 1));
                PushIfNeeded(stack, rc + int2(0, 1));

                rc.x += (rc.y & 1) == 0 ? 1 : -1;
                PushIfNeeded(stack, rc - int2(0, 1));
                PushIfNeeded(stack, rc + int2(0, 1));
            }
        }

        private void PushIfNeeded(NativeArray<int2> stack, int2 rc)
        {
            if (Grid.TryGetCellIndex(rc.x, rc.y, out int i))
            {
                var state = Grid[i];
                if (state.IsNot(CellState.MarkedOrRevealed))
                {
                    if (state == CellState.Zero)
                    {
                        stack[_stackSize++] = rc;
                    }

                    Grid.RevealedCellCount += 1;
                    Grid[i] = state.With(CellState.Revealed);
                }
            }
        }
    }
}