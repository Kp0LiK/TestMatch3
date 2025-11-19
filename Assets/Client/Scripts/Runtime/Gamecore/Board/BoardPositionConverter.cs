using UnityEngine;

namespace MiniIT.BOARD
{
    public class BoardPositionConverter
    {
        private readonly int _width;
        private readonly int _height;
        private readonly float _cellSize;
        private readonly float _boardWidth;
        private readonly float _boardHeight;
        private readonly float _spacing;

        public BoardPositionConverter(
            int width,
            int height,
            float cellSize,
            float boardWidth = 0f,
            float boardHeight = 0f,
            float spacing = 0f)
        {
            _width = width;
            _height = height;
            _cellSize = cellSize;
            _boardWidth = boardWidth;
            _boardHeight = boardHeight;
            _spacing = spacing;
        }

        public Vector2 GridToAnchoredPosition(Vector2Int gridPosition)
        {
            float xPos = (-_boardWidth / 2f) + _cellSize / 2f + gridPosition.x * (_cellSize + _spacing);
            float yPos = (-_boardHeight / 2f) + _cellSize / 2f + gridPosition.y * (_cellSize + _spacing);
            return new Vector2(xPos, yPos);
        }

        public float GetCellSize()
        {
            return _cellSize;
        }

        private Vector2Int AnchoredPositionToGrid(Vector2 anchoredPosition)
        {
            float relativeX = anchoredPosition.x - (-_boardWidth / 2f + _cellSize / 2f);
            float relativeY = anchoredPosition.y - (-_boardHeight / 2f + _cellSize / 2f);

            int gridX = Mathf.RoundToInt(relativeX / (_cellSize + _spacing));
            int gridY = Mathf.RoundToInt(relativeY / (_cellSize + _spacing));

            return new Vector2Int(gridX, gridY);
        }

        public Vector2Int WorldToGridPosition(Vector3 worldPosition)
        {
            return AnchoredPositionToGrid(new Vector2(worldPosition.x, worldPosition.y));
        }

        public bool IsValidGridPosition(Vector2Int gridPosition)
        {
            return gridPosition.x >= 0 && gridPosition.x < _width &&
                   gridPosition.y >= 0 && gridPosition.y < _height;
        }
    }
}