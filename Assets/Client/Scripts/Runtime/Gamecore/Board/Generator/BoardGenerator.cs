using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MiniIT.TILES;
using MiniIT.TILES.FACTORY;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MiniIT.BOARD
{
    public class BoardGenerator
    {
        private readonly BoardConfig _config;
        private readonly ITileFactory _tileFactory;
        private readonly TileBase[,] _tiles;
        private readonly int _width;
        private readonly int _height;

        private const float SPAWN_DURATION = 0.01f;

        public BoardGenerator(BoardConfig config, ITileFactory tileFactory, TileBase[,] tiles)
        {
            _config = config;
            _tileFactory = tileFactory;
            _tiles = tiles;
            _width = config.BoardWidth;
            _height = config.BoardHeight;
        }

        public async UniTask GenerateBoardAsync()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    int colorId = GenerateColorWithoutMatches(x, y);
                    await CreateTileAsync(x, y, colorId);
                }
            }
        }

        public async UniTask<BasicTile> CreateTileAsync(int x, int y, int colorId)
        {
            Vector2Int gridPosition = new Vector2Int(x, y);
            Sprite sprite = _config.TileSprites[colorId % _config.TileSprites.Length];

            BasicTile tile = await _tileFactory.CreateTileAsync(colorId, gridPosition, sprite);
            _tiles[x, y] = tile;

            await tile.AnimateSpawnAsync(SPAWN_DURATION);
            return tile;
        }

        public int GenerateColorWithoutMatches(int x, int y)
        {
            var excludedColors = new HashSet<int>();

            // Check left neighbors
            if (x >= 2)
            {
                var left1 = _tiles[x - 1, y];
                var left2 = _tiles[x - 2, y];
                if (left1 != null && left2 != null && left1.ColorId == left2.ColorId)
                {
                    excludedColors.Add(left1.ColorId);
                }
            }

            // Check bottom neighbors
            if (y >= 2)
            {
                var bottom1 = _tiles[x, y - 1];
                var bottom2 = _tiles[x, y - 2];
                if (bottom1 != null && bottom2 != null && bottom1.ColorId == bottom2.ColorId)
                {
                    excludedColors.Add(bottom1.ColorId);
                }
            }

            // Generate random color
            var availableColors = Enumerable.Range(0, _config.ColorsCount)
                .Where(c => !excludedColors.Contains(c))
                .ToList();

            if (availableColors.Count == 0)
                availableColors = Enumerable.Range(0, _config.ColorsCount).ToList();

            return availableColors[Random.Range(0, availableColors.Count)];
        }
    }
}

