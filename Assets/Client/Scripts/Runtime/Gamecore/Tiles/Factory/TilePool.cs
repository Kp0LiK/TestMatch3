using System.Collections.Generic;
using MiniIT.TILES;
using UnityEngine;

namespace MiniIT.TILES.FACTORY
{
    public class TilePool
    {
        private readonly Transform _parent;
        private readonly BasicTile _prefab;
        private readonly Queue<BasicTile> _pool;
        private readonly int _initialSize;
        private readonly int _maxSize;

        public TilePool(BasicTile prefab, Transform parent, int initialSize = 10, int maxSize = 50)
        {
            _prefab = prefab;
            _parent = parent;
            _initialSize = initialSize;
            _maxSize = maxSize;
            _pool = new Queue<BasicTile>();

            PrewarmPool();
        }

        private void PrewarmPool()
        {
            for (int i = 0; i < _initialSize; i++)
            {
                var tile = CreateNewTile();
                tile.gameObject.SetActive(false);
                _pool.Enqueue(tile);
            }
        }

        public BasicTile Get()
        {
            if (_pool.Count > 0)
            {
                var tile = _pool.Dequeue();
                tile.gameObject.SetActive(true);
                return tile;
            }

            return CreateNewTile();
        }

        public void Return(BasicTile tile)
        {
            if (tile == null)
                return;

            if (_pool.Count >= _maxSize)
            {
                Object.Destroy(tile.gameObject);
                return;
            }

            tile.gameObject.SetActive(false);
            tile.transform.SetParent(_parent);
            _pool.Enqueue(tile);
        }

        public void Clear()
        {
            while (_pool.Count > 0)
            {
                var tile = _pool.Dequeue();
                if (tile != null)
                {
                    Object.Destroy(tile.gameObject);
                }
            }
        }

        private BasicTile CreateNewTile()
        {
            return Object.Instantiate(_prefab, _parent);
        }
    }
}

