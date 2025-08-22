using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace CatlikeCodings.Prototypes.Maze2
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-22 20:41:22
    public partial struct OcclusionJob : IJobFor
    {
        [NativeDisableParallelForRestriction] public NativeArray<bool> IsVisibleToPlayer;
        public int IsVisibleToPlayerIndex;
        public Maze Maze;
        public float2 Position;
        public float WallExtents;
        public FieldOfView FieldOfView;
        public MazeFlags VisibilityFlag;
        public float Range;

        public void Execute(int i)
        {
            if (!TryGetFirstScan(i, out var scan, out var quadrant, out var origin))
            {
                return;
            }

            var stepEast = quadrant.FlipEW ? Maze.StepW : Maze.StepE;
            var stepNorth = quadrant.FlipNS ? Maze.StepS : Maze.StepN;
            var spottedFlags = MazeFlags.Empty;
            var stack = new ScanStack(Maze.SizeEW, scan);
            while (stack.TryPop(out scan))
            {
                var data = new CellData(scan.Offset - origin, WallExtents, scan.LeftSlope, scan.RightSlope);
                var currentIndex = scan.Index;
                var currentXOffset = scan.Offset.x;
                var isPreviousNorthVisible = false;
                var previousRightSlopeForNorthNeighbor = 0f;

                while (true)
                {
                    var cell = Maze.Set(currentIndex, VisibilityFlag);
                    data.UpdateForNextCell(cell, quadrant, FieldOfView.Range);
                    spottedFlags |= cell;

                    if (data.IsNorthVisible)
                    {
                        if (!isPreviousNorthVisible)
                        {
                            scan.ShiftEast(currentIndex, currentXOffset, data.LeftSlope);
                        }
                        else if (cell.HasNot(quadrant.Northwest))
                        {
                            stack.Push(scan.ShiftedNorth(stepNorth, previousRightSlopeForNorthNeighbor));
                            scan.ShiftEast(currentIndex, currentXOffset, data.LeftSlope);
                        }
                    }
                    else if (isPreviousNorthVisible)
                    {
                        stack.Push(scan.ShiftedNorth(stepNorth, previousRightSlopeForNorthNeighbor));
                    }

                    if (data.IsEastVisible)
                    {
                        currentIndex += stepEast;
                        currentXOffset += 1f;
                        isPreviousNorthVisible = data.IsNorthVisible;
                        previousRightSlopeForNorthNeighbor = data.RightSlopeForNorthNeighbor;
                        data.StepEast();
                    }
                    else
                    {
                        if (data.IsNorthVisible)
                        {
                            stack.Push(scan.ShiftedNorth(stepNorth, data.RightSlopeForNorthNeighbor));
                        }

                        break;
                    }
                }
            }

            if (VisibilityFlag != MazeFlags.VisibleToPlayer && spottedFlags.Has(MazeFlags.VisibleToPlayer))
            {
                IsVisibleToPlayer[IsVisibleToPlayerIndex] = true;
            }
        }

        private bool TryGetFirstScan(int i, out Scan scan, out Quadrant quadrant, out float2 origin)
        {
            quadrant = Quadrants[i];
            origin = frac(Position);
            scan = new Scan
            {
                Index = Maze.CoordinatesToIndex((int2)Position),
                RightSlope = float.MaxValue
            };
            float2 leftLine, rightLine;
            if (quadrant.FlipEW != quadrant.FlipNS)
            {
                leftLine = FieldOfView.RightLine;
                rightLine = FieldOfView.LeftLine;
            }
            else
            {
                leftLine = FieldOfView.LeftLine;
                rightLine = FieldOfView.RightLine;
            }

            if (quadrant.FlipEW)
            {
                leftLine.x = -leftLine.x;
                rightLine.x = -rightLine.x;
                origin.x = min(1f - origin.x, 0.999999f);
            }

            if (quadrant.FlipNS)
            {
                leftLine.y = -leftLine.y;
                rightLine.y = -rightLine.y;
                origin.y = min(1f - origin.y, 0.999999f);
            }

            if (FieldOfView.Omnidirectional)
            {
                return true;
            }

            if (
                leftLine.x >= 0f && leftLine.y <= 0f ||
                rightLine.x <= 0f && rightLine.y >= 0f ||
                leftLine.y <= 0f && rightLine.x <= 0f)
            {
                return false;
            }

            scan.LeftSlope = leftLine.x <= 0f ? 0f : leftLine.x / leftLine.y;
            scan.RightSlope = rightLine.y <= 0f ? float.MaxValue : rightLine.x / rightLine.y;
            return true;
        }
    }
}