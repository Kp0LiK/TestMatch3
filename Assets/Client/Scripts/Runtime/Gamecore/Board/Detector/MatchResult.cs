using System.Collections.Generic;
using UnityEngine;

namespace MiniIT.BOARD
{
    public class MatchResult
    {
        public HashSet<Vector2Int> MatchedPositions { get; }
        public int MatchCount => MatchedPositions.Count;
        public bool HasMatch => MatchCount >= 3;
        
        public MatchResult()
        {
            MatchedPositions = new HashSet<Vector2Int>();
        }

        public void AddPosition(Vector2Int position)
        {
            MatchedPositions.Add(position);
        }

        public void Merge(MatchResult other)
        {
            foreach (var pos in other.MatchedPositions)
            {
                MatchedPositions.Add(pos);
            }
        }
    }
}