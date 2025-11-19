using System;
using System.Reflection;
using Cysharp.Threading.Tasks;
using MiniIT.BOARD;
using UnityEngine;
using Zenject;

namespace MiniIT.TILES.FACTORY
{
    public class TileFactory : ITileFactory
    {
        private readonly TilePool _pool;
        private readonly DiContainer _container;
        private readonly Type _tileType;
        private readonly Board _board;

        public TileFactory(
            BasicTile prefab,
            Transform parent,
            DiContainer container,
            Board board,
            int initialPoolSize = 10,
            int maxPoolSize = 50)
        {
            _container = container;
            _board = board;
            _pool = new TilePool(prefab, parent, initialPoolSize, maxPoolSize);
            _tileType = prefab.GetType();
        }

        public async UniTask<BasicTile> CreateTileAsync(int colorId, Vector2Int position, Sprite sprite)
        {
            var tile = _pool.Get();

            var positionConverter = _board?.GetPositionConverter();
            if (positionConverter != null)
            {
                Vector2 anchoredPos = positionConverter.GridToAnchoredPosition(position);
                tile.RectTransform.anchoredPosition = anchoredPos;

                float cellSize = positionConverter.GetCellSize();
                tile.RectTransform.sizeDelta = new Vector2(cellSize, cellSize);
            }
            else
            {
                Debug.LogError("[TileFactory] PositionConverter is null");
            }

            InitializeTile(tile, colorId, position, sprite);
            _container.Inject(tile);

            await UniTask.Yield();
            return tile;
        }

        public void ReturnTile(BasicTile tile)
        {
            if (tile == null)
                return;

            ResetTile(tile);
            _pool.Return(tile);
        }

        public void ClearPool()
        {
            _pool.Clear();
        }

        private void InitializeTile(BasicTile tile, int colorId, Vector2Int position, Sprite sprite)
        {
            // Use Reflection to call Init method
            MethodInfo initMethod = _tileType.GetMethod("Init",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new[] { typeof(int), typeof(Vector2Int), typeof(Sprite) },
                null);

            if (initMethod != null)
            {
                initMethod.Invoke(tile, new object[] { colorId, position, sprite });
            }
            else
            {
                // Fallback
                tile.Init(colorId, position, sprite);
            }
        }

        private void ResetTile(BasicTile tile)
        {
            // Reset tile state using Reflection
            var stateProperty = _tileType.GetProperty("State",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            //Set tile state using Reflection
            if (stateProperty != null)
            {
                var setStateMethod = _tileType.GetMethod("SetState",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                if (setStateMethod != null)
                {
                    var idleState = Enum.Parse(typeof(TileState), "Idle");
                    setStateMethod.Invoke(tile, new[] { idleState });
                }

                //Ser tile position using Reflection
                var setPositionMethod = _tileType.GetMethod("SetPosition",
                    BindingFlags.Public | BindingFlags.Instance);
                setPositionMethod?.Invoke(tile, new object[] { Vector2Int.zero });

                // Reset transform
                tile.transform.localPosition = Vector3.zero;
                tile.transform.localScale = Vector3.one;
                tile.transform.localRotation = Quaternion.identity;
            }
        }
    }
}