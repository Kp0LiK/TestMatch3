using System.Collections.Generic;
using MiniIT.TILES;
using UnityEngine;

namespace MiniIT.BOARD
{
    public class SimpleMatchDetector : IMatchDetector
    {
        private readonly int _width;
        private readonly int _height;

        public SimpleMatchDetector(int width, int height)
        {
            _width = width;
            _height = height;
        }

        public MatchResult FindMatches(TileBase[,] board)
        {
            var result = new MatchResult();

            // Check horizontal matches
            for (int y = 0; y < _height; y++)
            {
                var horizontalMatches = FindMatchesInLine(board, y, true);
                result.Merge(horizontalMatches);
            }

            // Check vertical matches
            for (int x = 0; x < _width; x++)
            {
                var verticalMatches = FindMatchesInLine(board, x, false);
                result.Merge(verticalMatches);
            }

            return result;
        }

        private MatchResult FindMatchesInLine(TileBase[,] board, int index, bool isHorizontal)
        {
            var result = new MatchResult();
            var currentColor = -1;
            var matchStart = 0;
            var matchCount = 0;

            int lineLength = isHorizontal ? _width : _height;

            for (int i = 0; i <= lineLength; i++)
            {
                Vector2Int position = isHorizontal ? new Vector2Int(i, index) : new Vector2Int(index, i);
                int colorId = GetColorIdAtPosition(board, position);

                if (i < lineLength && colorId == currentColor && colorId != -1)
                {
                    matchCount++;
                }
                else
                {
                    // Check if we have a match
                    if (matchCount >= 3)
                    {
                        for (int j = matchStart; j < matchStart + matchCount; j++)
                        {
                            Vector2Int matchPos = isHorizontal ? new Vector2Int(j, index) : new Vector2Int(index, j);
                            result.AddPosition(matchPos);
                        }
                    }

                    // Start new match
                    if (i < lineLength)
                    {
                        currentColor = colorId;
                        matchStart = i;
                        matchCount = colorId != -1 ? 1 : 0;
                    }
                }
            }

            return result;
        }

        public MatchResult FindMatchesAtPosition(TileBase[,] board, Vector2Int position)
        {
            var result = new MatchResult();

            if (!IsValidPosition(position))
                return result;

            int colorId = GetColorIdAtPosition(board, position);
            if (colorId == -1)
                return result;

            // Check horizontal line
            var horizontalMatches = FindMatchesInDirection(board, position, Vector2Int.left, colorId);
            horizontalMatches.UnionWith(FindMatchesInDirection(board, position, Vector2Int.right, colorId));
            if (horizontalMatches.Count >= 2)
            {
                horizontalMatches.Add(position);
                if (horizontalMatches.Count >= 3)
                {
                    var horizontalResult = new MatchResult();
                    foreach (var pos in horizontalMatches)
                    {
                        horizontalResult.AddPosition(pos);
                    }

                    result.Merge(horizontalResult);
                }
            }

            // Check vertical line
            var verticalMatches = FindMatchesInDirection(board, position, Vector2Int.up, colorId);
            verticalMatches.UnionWith(FindMatchesInDirection(board, position, Vector2Int.down, colorId));
            if (verticalMatches.Count >= 2)
            {
                verticalMatches.Add(position);
                if (verticalMatches.Count >= 3)
                {
                    var verticalResult = new MatchResult();
                    foreach (var pos in verticalMatches)
                    {
                        verticalResult.AddPosition(pos);
                    }

                    result.Merge(verticalResult);
                }
            }

            return result;
        }

        private HashSet<Vector2Int> FindMatchesInDirection(TileBase[,] board, Vector2Int start, Vector2Int direction,
            int colorId)
        {
            var matches = new HashSet<Vector2Int>();
            Vector2Int current = start + direction;

            while (IsValidPosition(current))
            {
                int currentColorId = GetColorIdAtPosition(board, current);
                if (currentColorId == colorId)
                {
                    matches.Add(current);
                    current += direction;
                }
                else
                {
                    break;
                }
            }

            return matches;
        }

        public bool WouldCreateMatch(TileBase[,] board, Vector2Int pos1, Vector2Int pos2)
        {
            // Simulate swap
            SwapTilesInArray(board, pos1, pos2);

            // Check if swap creates matches
            var matches1 = FindMatchesAtPosition(board, pos1);
            var matches2 = FindMatchesAtPosition(board, pos2);

            // Restore swap
            SwapTilesInArray(board, pos1, pos2);

            return matches1.HasMatch || matches2.HasMatch;
        }

        private void SwapTilesInArray(TileBase[,] board, Vector2Int pos1, Vector2Int pos2)
        {
            (board[pos1.x, pos1.y], board[pos2.x, pos2.y]) = (board[pos2.x, pos2.y], board[pos1.x, pos1.y]);
        }

        private int GetColorIdAtPosition(TileBase[,] board, Vector2Int position)
        {
            if (!IsValidPosition(position))
                return -1;

            var tile = board[position.x, position.y];
            return tile != null && tile.IsMatchable() ? tile.ColorId : -1;
        }

        private bool IsValidPosition(Vector2Int position)
        {
            return position.x >= 0 && position.x < _width &&
                   position.y >= 0 && position.y < _height;
        }
    }
}