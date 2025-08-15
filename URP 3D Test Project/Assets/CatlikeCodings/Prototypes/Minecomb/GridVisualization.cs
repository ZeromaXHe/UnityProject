using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace CatlikeCodings.Prototypes.Minecomb
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-15 20:47:15
    public struct GridVisualization
    {
        public const int RowsPerCell = 7,
            ColumnsPerCell = 5,
            BlocksPerCell = RowsPerCell * ColumnsPerCell;

        private static readonly int PositionsId = Shader.PropertyToID("_Positions");
        private static readonly int ColorsId = Shader.PropertyToID("_Colors");

        private ComputeBuffer _positionsBuffer, _colorsBuffer;
        private NativeArray<float3> _positions, _colors, _ripples;
        private int _rippleCount;
        private Grid _grid;
        private Material _material;
        private Mesh _mesh;

        public void Initialize(Grid grid, Material material, Mesh mesh)
        {
            _grid = grid;
            _material = material;
            _mesh = mesh;

            var instanceCount = grid.CellCount * BlocksPerCell;
            _positions = new NativeArray<float3>(instanceCount, Allocator.Persistent);
            _colors = new NativeArray<float3>(instanceCount, Allocator.Persistent);
            _ripples = new NativeArray<float3>(10, Allocator.Persistent);
            _rippleCount = 0;
            _positionsBuffer = new ComputeBuffer(instanceCount, 3 * 4);
            _colorsBuffer = new ComputeBuffer(instanceCount, 3 * 4);
            material.SetBuffer(PositionsId, _positionsBuffer);
            material.SetBuffer(ColorsId, _colorsBuffer);

            new InitializeVisualizationJob
            {
                Positions = _positions,
                Colors = _colors,
                Rows = grid.Rows,
                Columns = grid.Columns
            }.ScheduleParallel(grid.CellCount, grid.Columns, default).Complete();
            _positionsBuffer.SetData(_positions);
            _colorsBuffer.SetData(_colors);
        }

        public void Dispose()
        {
            _positions.Dispose();
            _colors.Dispose();
            _ripples.Dispose();
            _positionsBuffer.Release();
            _colorsBuffer.Release();
        }

        public void Draw()
        {
            if (_rippleCount > 0)
            {
                Update();
            }

            Graphics.DrawMeshInstancedProcedural(_mesh, 0, _material,
                new Bounds(Vector3.zero, Vector3.one), _positionsBuffer.count);
        }

        private void Update()
        {
            var dt = Time.deltaTime;
            for (var i = 0; i < _rippleCount; i++)
            {
                var ripple = _ripples[i];
                if (ripple.z < 1f)
                {
                    ripple.z = Mathf.Min(ripple.z + dt, 1f);
                    _ripples[i] = ripple;
                }
                else
                {
                    _ripples[i] = _ripples[--_rippleCount];
                    i -= 1;
                }
            }

            new UpdateVisualizationJob
            {
                Positions = _positions,
                Colors = _colors,
                Ripples = _ripples,
                RippleCount = _rippleCount,
                Grid = _grid
            }.ScheduleParallel(_grid.CellCount, _grid.Columns, default).Complete();
            _positionsBuffer.SetData(_positions);
            _colorsBuffer.SetData(_colors);
        }

        public bool TryGetHitCellIndex(Ray ray, out int cellIndex)
        {
            var p = ray.origin - ray.direction * (ray.origin.y / ray.direction.y);

            var x = p.x + ColumnsPerCell / 2 + 1.5f;
            x /= ColumnsPerCell + 1;
            x += (_grid.Columns - 1) * 0.5f;
            var c = Mathf.FloorToInt(x);

            var z = p.z + RowsPerCell / 2f + 1.5f;
            z /= RowsPerCell + 1;
            z += (_grid.Rows - 1) * 0.5f + (c & 1) * 0.5f - 0.25f;
            var r = Mathf.FloorToInt(z);

            var valid = _grid.TryGetCellIndex(r, c, out cellIndex) &&
                        x - c > 1f / (ColumnsPerCell + 1) &&
                        z - r > 1f / (RowsPerCell + 1);
            if (valid && _rippleCount < _ripples.Length)
            {
                _ripples[_rippleCount++] = float3(p.x, p.z, 0f);
            }

            return valid;
        }
    }
}