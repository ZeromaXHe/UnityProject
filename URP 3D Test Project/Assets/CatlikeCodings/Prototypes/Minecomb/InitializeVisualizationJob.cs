using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace CatlikeCodings.Prototypes.Minecomb
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-15 21:04:15
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
    public struct InitializeVisualizationJob : IJobFor
    {
        [WriteOnly, NativeDisableParallelForRestriction]
        public NativeArray<float3> Positions, Colors;

        public int Rows, Columns;

        public void Execute(int i)
        {
            var cellPosition = GetCellPosition(i);
            var blockOffset = i * GridVisualization.BlocksPerCell;

            for (var bi = 0; bi < GridVisualization.BlocksPerCell; bi++)
            {
                Positions[blockOffset + bi] = cellPosition + GetBlockPosition(bi);
                Colors[blockOffset + bi] = 0.5f;
            }
        }

        private static float3 GetBlockPosition(int i)
        {
            var r = i / GridVisualization.ColumnsPerCell;
            var c = i - r * GridVisualization.ColumnsPerCell;
            return float3(c, 0f, r);
        }

        private float3 GetCellPosition(int i)
        {
            var r = i / Columns;
            var c = i - r * Columns;
            return float3(c - (Columns - 1) * 0.5f, 0f, r - (Rows - 1) * 0.5f - (c & 1) * 0.5f + 0.25f) *
                   float3(GridVisualization.ColumnsPerCell + 1, 0f, GridVisualization.RowsPerCell + 1) -
                   float3(GridVisualization.ColumnsPerCell / 2, 0f, GridVisualization.RowsPerCell / 2);
        }
    }
}