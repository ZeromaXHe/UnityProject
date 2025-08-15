using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace CatlikeCodings.Prototypes.Minecomb
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-15 20:45:15
    public struct Grid
    {
        public CellState this[int i]
        {
            get => _states[i];
            set => _states[i] = value;
        }

        public int Rows { get; private set; }
        public int Columns { get; private set; }
        public int CellCount => _states.Length;

        public int HiddenCellCount => CellCount - RevealedCellCount;

        public int RevealedCellCount
        {
            get => _revealedCellCount[0];
            set => _revealedCellCount[0] = value;
        }

        private NativeArray<int> _revealedCellCount;
        private NativeArray<CellState> _states;

        public void Initialize(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            _revealedCellCount = new NativeArray<int>(1, Allocator.Persistent);
            _states = new NativeArray<CellState>(Rows * Columns, Allocator.Persistent);
        }

        public void Dispose()
        {
            _revealedCellCount.Dispose();
            _states.Dispose();
        }

        public int GetCellIndex(int row, int column) => row * Columns + column;

        public bool TryGetCellIndex(int row, int column, out int index)
        {
            bool valid = 0 <= row && row < Rows && 0 <= column && column < Columns;
            index = valid ? GetCellIndex(row, column) : -1;
            return valid;
        }

        public void GetRowColumn(int index, out int row, out int column)
        {
            row = index / Columns;
            column = index - row * Columns;
        }

        public void PlaceMines(int mines) => new PlaceMinesJob
        {
            Grid = this,
            Mines = mines,
            Seed = Random.Range(1, int.MaxValue)
        }.Schedule().Complete();

        public void Reveal(int index)
        {
            var job = new RevealRegionJob
            {
                Grid = this
            };
            GetRowColumn(index, out job.StartRowColumn.x, out job.StartRowColumn.y);
            job.Schedule().Complete();
        }

        public void RevealMinesAndMistakes() => new RevealMinesAndMistakesJob
        {
            Grid = this
        }.ScheduleParallel(CellCount, Columns, default).Complete();
    }
}