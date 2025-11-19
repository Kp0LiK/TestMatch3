using Cysharp.Threading.Tasks;
using MiniIT.TILES;

namespace MiniIT.BOARD
{
    public class TileSelectionManager
    {
        private readonly Board _board;
        private BasicTile _selectedTile;

        public TileSelectionManager(Board board)
        {
            _board = board;
        }

        public void OnTileSelected(BasicTile tile)
        {
            if (_selectedTile == null)
            {
                _selectedTile = tile;
            }
            else if (_selectedTile != tile)
            {
                // Try to swap
                var pos1 = _selectedTile.Position;
                var pos2 = tile.Position;
                _board.TrySwapAsync(pos1, pos2).Forget();
                _selectedTile = null;
            }
        }

        public void OnTileDeselected(BasicTile tile)
        {
            if (_selectedTile == tile)
            {
                _selectedTile = null;
            }
        }

        public void Reset()
        {
            _selectedTile = null;
        }
    }
}


