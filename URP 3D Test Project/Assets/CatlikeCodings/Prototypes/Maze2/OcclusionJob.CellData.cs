using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace CatlikeCodings.Prototypes.Maze2
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-22 20:50:22
    public partial struct OcclusionJob
    {
        struct CellData
        {
            private readonly float _inset;
            private readonly float _south, _north;
            private float _west, _east;

            private readonly float WestInset => _west + _inset;
            private readonly float EastInset => _east - _inset;
            private readonly float SouthInset => _south + _inset;
            private readonly float NorthInset => _north - _inset;

            public bool IsNorthVisible { get; private set; }
            public bool IsEastVisible { get; private set; }
            public float LeftSlope { get; private set; }
            public float RightSlope { get; private set; }
            public float RightSlopeForNorthNeighbor { get; private set; }

            public CellData(float2 offset, float inset, float leftSlope, float rightSlope)
            {
                this = default;
                _inset = inset;
                _west = offset.x;
                _east = _west + 1f;
                var south1 = offset.y;
                _north = south1 + 1f;
                LeftSlope = leftSlope;
                RightSlope = rightSlope;
            }

            public void StepEast()
            {
                _west += 1f;
                _east += 1f;
            }

            public void UpdateForNextCell(MazeFlags cell, Quadrant quadrant, float range)
            {
                if (cell.Has(quadrant.South) &&
                    cell.HasNot(quadrant.Southeast) &&
                    SouthInset > 0f)
                {
                    RightSlope = min(RightSlope, EastInset / SouthInset);
                }

                if (cell.Has(quadrant.North) && IsInRange(max(0f, _west), _north, range))
                {
                    if (cell.HasNot(quadrant.Northwest) && WestInset > 0f)
                    {
                        LeftSlope = max(LeftSlope, WestInset / NorthInset);
                    }

                    RightSlopeForNorthNeighbor = min(RightSlope,
                        (cell.Has(quadrant.Northeast) ? _east : EastInset) / _north);

                    IsNorthVisible = LeftSlope < RightSlopeForNorthNeighbor;
                }
                else
                {
                    IsNorthVisible = false;
                }

                IsEastVisible =
                    cell.Has(quadrant.East) && LeftSlope < RightSlope &&
                    IsInRange(_east, max(0f, _south), range) && (cell.Has(quadrant.Northeast)
                        ? _east / _north < RightSlope
                        : NorthInset > 0f && _east / NorthInset < RightSlope);
            }

            private static bool IsInRange(float x, float y, float range) => x * x + y * y < range * range;
        }
    }
}