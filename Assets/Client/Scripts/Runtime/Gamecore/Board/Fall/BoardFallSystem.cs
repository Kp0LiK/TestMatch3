using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MiniIT.TILES;
using UnityEngine;

namespace MiniIT.BOARD
{
    public class BoardFallSystem
    {
        private readonly BoardConfig _config;
        private readonly BoardPositionConverter _positionConverter;
        private readonly TileBase[,] _tiles;
        private readonly int _width;
        private readonly int _height;

        public BoardFallSystem(
            BoardConfig config,
            BoardPositionConverter positionConverter,
            TileBase[,] tiles)
        {
            _config = config;
            _positionConverter = positionConverter;
            _tiles = tiles;
            _width = config.BoardWidth;
            _height = config.BoardHeight;
        }

        public async UniTask MakeTilesFallAsync()
        {
            var fallTasks = new List<UniTask>();

            for (int x = 0; x < _width; x++)
            {
                int writeIndex = 0;

                for (int y = 0; y < _height; y++)
                {
                    var tile = _tiles[x, y];

                    if (tile != null)
                    {
                        if (y != writeIndex)
                        {
                            Vector2Int newPosition = new Vector2Int(x, writeIndex);

                            // Move tile in array
                            _tiles[x, writeIndex] = tile;
                            _tiles[x, y] = null;

                            // Update position
                            tile.SetPosition(newPosition);

                            // Animate fall using anchored positions for UI
                            Vector2 targetAnchoredPos = _positionConverter.GridToAnchoredPosition(newPosition);
                            if (tile is BasicTile basicTile)
                            {
                                fallTasks.Add(basicTile.AnimateFallAsync(targetAnchoredPos, _config.FallDuration));
                            }
                        }

                        writeIndex++;
                    }
                }
            }

            await UniTask.WhenAll(fallTasks);
        }
    }
}