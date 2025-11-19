using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MiniIT.AUDIO;
using MiniIT.SCORE;
using MiniIT.TILES;
using MiniIT.TILES.FACTORY;

namespace MiniIT.BOARD
{
    public class BoardMatchProcessor
    {
        private readonly IMatchDetector _matchDetector;
        private readonly TileBase[,] _tiles;
        private readonly ITileFactory _tileFactory;
        private readonly BoardFallSystem _fallSystem;
        private readonly BoardSpawnSystem _spawnSystem;
        private readonly Action<BasicTile> _onTileSelectedUnsubscribe;
        private readonly Action<BasicTile> _onTileDeselectedUnsubscribe;
        private readonly AudioSystem _audioSystem;
        private readonly ScoreSystem _scoreSystem;

        public event Action<MatchResult> MatchesFound;

        public BoardMatchProcessor(
            IMatchDetector matchDetector,
            TileBase[,] tiles,
            ITileFactory tileFactory,
            BoardFallSystem fallSystem,
            BoardSpawnSystem spawnSystem,
            Action<BasicTile> onTileSelectedUnsubscribe,
            Action<BasicTile> onTileDeselectedUnsubscribe,
            AudioSystem audioSystem = null,
            ScoreSystem scoreSystem = null)
        {
            _matchDetector = matchDetector;
            _tiles = tiles;
            _tileFactory = tileFactory;
            _fallSystem = fallSystem;
            _spawnSystem = spawnSystem;
            _onTileSelectedUnsubscribe = onTileSelectedUnsubscribe;
            _onTileDeselectedUnsubscribe = onTileDeselectedUnsubscribe;
            _audioSystem = audioSystem;
            _scoreSystem = scoreSystem;
        }

        public async UniTask ProcessMatchesAsync()
        {
            int comboLevel = 0;

            while (true)
            {
                var matches = _matchDetector.FindMatches(_tiles);

                if (!matches.HasMatch)
                    break;

                comboLevel++;


                _audioSystem.PlayComboSound(comboLevel);
                _scoreSystem.AddScore(matches.MatchCount, comboLevel);

                MatchesFound?.Invoke(matches);

                await DestroyMatchedTilesAsync(matches);
                await _fallSystem.MakeTilesFallAsync();
                await _spawnSystem.SpawnNewTilesAsync();
            }
        }

        private async UniTask DestroyMatchedTilesAsync(MatchResult matches)
        {
            var destroyTasks = new List<UniTask>();
            var tilesToDestroy = new List<BasicTile>();

            foreach (var position in matches.MatchedPositions)
            {
                var tile = _tiles[position.x, position.y] as BasicTile;
                if (tile != null)
                {
                    tilesToDestroy.Add(tile);
                    destroyTasks.Add(tile.AnimateDestroyAsync());
                }
            }

            await UniTask.WhenAll(destroyTasks);

            foreach (var position in matches.MatchedPositions)
            {
                var tile = _tiles[position.x, position.y] as BasicTile;
                if (tile != null)
                {
                    _onTileSelectedUnsubscribe?.Invoke(tile);
                    _onTileDeselectedUnsubscribe?.Invoke(tile);
                }

                _tiles[position.x, position.y] = null;
            }

            // Return tiles to pool
            foreach (var tile in tilesToDestroy)
            {
                if (tile != null && _tileFactory != null)
                {
                    _tileFactory.ReturnTile(tile);
                }
            }
        }
    }
}