using MiniIT.TILES;
using UnityEngine;

namespace MiniIT.BOARD
{
    public interface IMatchDetector
    {
        MatchResult FindMatches(TileBase[,] board);
        MatchResult FindMatchesAtPosition(TileBase[,] board, Vector2Int position);
        bool WouldCreateMatch(TileBase[,] board, Vector2Int pos1, Vector2Int pos2);
    }
}

