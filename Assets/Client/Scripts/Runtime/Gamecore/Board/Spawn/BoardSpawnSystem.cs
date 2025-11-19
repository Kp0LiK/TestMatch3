using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MiniIT.TILES;

namespace MiniIT.BOARD
{
    public class BoardSpawnSystem
    {
        private readonly BoardConfig _config;
        private readonly TileBase[,] _tiles;
        private readonly BoardGenerator _generator;
        private readonly int _width;
        private readonly int _height;

        public BoardSpawnSystem(
            BoardConfig config,
            TileBase[,] tiles,
            BoardGenerator generator)
        {
            _config = config;
            _tiles = tiles;
            _generator = generator;
            _width = config.BoardWidth;
            _height = config.BoardHeight;
        }

        public async UniTask SpawnNewTilesAsync()
        {
            var spawnTasks = new List<UniTask>();

            for (int x = 0; x < _width; x++)
            {
                for (int y = _height - 1; y >= 0; y--)
                {
                    if (_tiles[x, y] == null)
                    {
                        int colorId = _generator.GenerateColorWithoutMatches(x, y);
                        spawnTasks.Add(_generator.CreateTileAsync(x, y, colorId));
                    }
                }
            }

            await UniTask.WhenAll(spawnTasks);
        }
    }
}


