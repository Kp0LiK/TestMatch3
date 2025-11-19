using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MiniIT.TILES.FACTORY
{
    public interface ITileFactory
    {
        UniTask<BasicTile> CreateTileAsync(int colorId, Vector2Int position, Sprite sprite);
        void ReturnTile(BasicTile tile);
        void ClearPool();
    }
}


