using System;
using UnityEngine;

namespace MiniIT.SCORE
{
    public class ScoreSystem
    {
        private int _currentScore;
        private readonly int _basePointsPerTile;
        private readonly int _bonusPointsPerExtraTile;
        private readonly float[] _comboMultipliers;

        public int CurrentScore => _currentScore;

        public event Action<int> ScoreChanged;
        public event Action<int> ScoreAdded;

        public ScoreSystem(
            int basePointsPerTile = 10,
            int bonusPointsPerExtraTile = 5,
            float[] comboMultipliers = null)
        {
            _basePointsPerTile = basePointsPerTile;
            _bonusPointsPerExtraTile = bonusPointsPerExtraTile;

            if (comboMultipliers != null && comboMultipliers.Length > 0)
            {
                _comboMultipliers = comboMultipliers;
            }
            else
            {
                _comboMultipliers = new[]
                {
                    1.0f, 1.5f, 2.0f, 2.5f, 3.0f, 3.5f, 4.0f, 4.5f, 5.0f
                };
            }
        }

        public int CalculateScore(int matchCount, int comboLevel)
        {
            if (matchCount < 3)
                return 0;

            // Base score: points per tile
            int baseScore = _basePointsPerTile * matchCount;

            // Bonus score: extra points for tiles
            int bonusScore = _bonusPointsPerExtraTile * Mathf.Max(0, matchCount - 3);

            // Total before combo multiplier
            int totalScore = baseScore + bonusScore;

            // Apply combo multiplier
            float multiplier = 1.0f;
            if (_comboMultipliers != null && _comboMultipliers.Length > 0)
            {
                int safeComboLevel = Mathf.Max(1, comboLevel);
                int comboIndex = Mathf.Clamp(safeComboLevel - 1, 0, _comboMultipliers.Length - 1);
                multiplier = _comboMultipliers[comboIndex];
            }

            int finalScore = Mathf.RoundToInt(totalScore * multiplier);

            return finalScore;
        }

        public void AddScore(int matchCount, int comboLevel)
        {
            int pointsToAdd = CalculateScore(matchCount, comboLevel);
            AddScore(pointsToAdd);
        }

        public void AddScore(int points)
        {
            if (points <= 0)
                return;

            _currentScore += points;
            ScoreChanged?.Invoke(_currentScore);
            ScoreAdded?.Invoke(points);
        }

        public void Reset()
        {
            _currentScore = 0;
            ScoreChanged?.Invoke(_currentScore);
        }
    }
}