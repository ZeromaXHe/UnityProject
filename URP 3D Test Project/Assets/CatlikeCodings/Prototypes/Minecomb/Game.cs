using TMPro;
using UnityEngine;

namespace CatlikeCodings.Prototypes.Minecomb
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-15 20:43:00
    public class Game : MonoBehaviour
    {
        [SerializeField] private Material material;
        [SerializeField] private Mesh mesh;
        [SerializeField] private TextMeshPro minesText;
        [SerializeField, Min(1)] private int rows = 8, columns = 21, mines = 30;
        private Grid _grid;
        private GridVisualization _visualization;
        private int _markedSureCount;
        private bool _isGameOver;

        private void OnEnable()
        {
            _grid.Initialize(rows, columns);
            _visualization.Initialize(_grid, material, mesh);
            StartNewGame();
        }

        private void StartNewGame()
        {
            _isGameOver = false;
            mines = Mathf.Min(mines, _grid.CellCount);
            minesText.SetText("{0}", mines);
            _markedSureCount = 0;
            _grid.PlaceMines(mines);
        }

        private void OnDisable()
        {
            _grid.Dispose();
            _visualization.Dispose();
        }

        private void Update()
        {
            if (_grid.Rows != rows || _grid.Columns != columns)
            {
                OnDisable();
                OnEnable();
            }

            PerformAction();
            _visualization.Draw();
        }

        private void PerformAction()
        {
            var revealAction = Input.GetMouseButtonDown(0);
            var markAction = Input.GetMouseButtonDown(1);
            if ((revealAction || markAction) &&
                _visualization.TryGetHitCellIndex(Camera.main.ScreenPointToRay(Input.mousePosition), out var cellIndex))
            {
                if (_isGameOver)
                {
                    StartNewGame();
                }

                if (revealAction)
                {
                    DoRevealAction(cellIndex);
                }
                else
                {
                    DoMarkAction(cellIndex);
                }
            }
        }

        private void DoMarkAction(int cellIndex)
        {
            var state = _grid[cellIndex];
            if (state.Is(CellState.Revealed))
            {
                return;
            }

            if (state.IsNot(CellState.Marked))
            {
                _grid[cellIndex] = state.With(CellState.MarkedSure);
                _markedSureCount += 1;
            }
            else if (state.Is(CellState.MarkedSure))
            {
                _grid[cellIndex] =
                    state.Without(CellState.MarkedSure).With(CellState.MarkedUnsure);
                _markedSureCount -= 1;
            }
            else
            {
                _grid[cellIndex] = state.Without(CellState.MarkedUnsure);
            }

            minesText.SetText("{0}", mines - _markedSureCount);
        }

        private void DoRevealAction(int cellIndex)
        {
            var state = _grid[cellIndex];
            if (state.Is(CellState.MarkedOrRevealed))
            {
                return;
            }

            _grid.Reveal(cellIndex);
            if (state.Is(CellState.Mine))
            {
                _isGameOver = true;
                minesText.SetText("FAILURE");
                _grid.RevealMinesAndMistakes();
            }
            else if (_grid.HiddenCellCount == mines)
            {
                _isGameOver = true;
                minesText.SetText("SUCCESS");
            }
        }
    }
}