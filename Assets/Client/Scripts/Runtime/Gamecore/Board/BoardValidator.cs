using MiniIT.TILES;
using UnityEngine;

namespace MiniIT.BOARD
{
    public class BoardValidator
    {
        private readonly int _width;
        private readonly int _height;
        private readonly BoardPositionConverter _positionConverter;

        public BoardValidator(int width, int height, BoardPositionConverter positionConverter)
        {
            _width = width;
            _height = height;
            _positionConverter = positionConverter;
        }

        public bool IsValidPosition(Vector2Int position)
        {
            return position.x >= 0 && position.x < _width &&
                   position.y >= 0 && position.y < _height;
        }

        public bool AreAdjacent(Vector2Int pos1, Vector2Int pos2)
        {
            Vector2Int delta = pos1 - pos2;
            int distance = Mathf.Abs(delta.x) + Mathf.Abs(delta.y);
            return distance == 1;
        }

        public bool CanSwap(TileBase[,] tiles, Vector2Int pos1, Vector2Int pos2, bool isProcessing)
        {
            // Check if board is processing
            if (isProcessing)
                return false;

            // Check if positions are valid
            if (!IsValidPosition(pos1) || !IsValidPosition(pos2))
                return false;

            // Check if positions are adjacent
            if (!AreAdjacent(pos1, pos2))
                return false;

            // Check if tiles exist
            var tile1 = tiles[pos1.x, pos1.y];
            var tile2 = tiles[pos2.x, pos2.y];

            if (tile1 == null || tile2 == null)
                return false;

            // Check if tiles can interact
            if (!tile1.CanInteract() || !tile2.CanInteract())
                return false;

            return true;
        }

        public Vector2Int? GetAdjacentPosition(Vector2Int position, Vector2Int direction)
        {
            Vector2Int adjacentPos = position + direction;
            return IsValidPosition(adjacentPos) ? adjacentPos : null;
        }
    }
}