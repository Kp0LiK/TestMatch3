using System;
using Cysharp.Threading.Tasks;
using MiniIT.AUDIO;
using MiniIT.SCORE;
using MiniIT.TILES;
using MiniIT.TILES.FACTORY;
using UnityEngine;
using Zenject;

namespace MiniIT.BOARD
{
    public class Board : MonoBehaviour
    {
        [SerializeField] private BoardConfig _config;
        [SerializeField] private BoardView _boardView;
        [SerializeField] private BasicTile _tilePrefab;
        [SerializeField] private Transform _tilesParent;

        private TileBase[,] _tiles;
        private BoardPositionConverter _positionConverter;
        private BoardValidator _validator;
        private IMatchDetector _matchDetector;
        private ITileFactory _tileFactory;
        private BoardGenerator _generator;
        private BoardFallSystem _fallSystem;
        private BoardSpawnSystem _spawnSystem;
        private BoardMatchProcessor _matchProcessor;
        private TileSelectionManager _selectionManager;
        private AudioSystem _audioSystem;
        private ScoreSystem _scoreSystem;

        private bool _isProcessing;

        public event Action<MatchResult> MatchesFound;
        public event Action BoardReady;
        public event Action<bool> SwapCompleted;
        public event Action TilesSpawned;

        public int Width => _config.BoardWidth;
        public int Height => _config.BoardHeight;
        public bool IsProcessing => _isProcessing;

        [Inject]
        private void Construct(AudioSystem audioSystem, ScoreSystem scoreSystem)
        {
            _audioSystem = audioSystem;
            _scoreSystem = scoreSystem;
        }

        private void Awake()
        {
            if (_tilesParent == null)
                _tilesParent = transform;

            _tiles = new TileBase[_config.BoardWidth, _config.BoardHeight];

            _matchDetector = new SimpleMatchDetector(_config.BoardWidth, _config.BoardHeight);
            _selectionManager = new TileSelectionManager(this);
        }

        private void OnDestroy()
        {
            UnsubscribeFromAllTiles();
        }

        private void UnsubscribeFromAllTiles()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var tile = _tiles[x, y] as BasicTile;
                    if (tile != null)
                    {
                        UnsubscribeFromTile(tile);
                    }
                }
            }
        }

        private void SubscribeToTile(BasicTile tile)
        {
            tile.TileSelected += _selectionManager.OnTileSelected;
            tile.TileDeselected += _selectionManager.OnTileDeselected;
        }

        private void UnsubscribeFromTile(BasicTile tile)
        {
            tile.TileSelected -= _selectionManager.OnTileSelected;
            tile.TileDeselected -= _selectionManager.OnTileDeselected;
        }

        public async UniTask InitializeAsync()
        {
            _isProcessing = true;
            _boardView.InitializePositioning();

            InitializeSystems();

            await _generator.GenerateBoardAsync();
            SubscribeToAllTiles();

            await _matchProcessor.ProcessMatchesAsync();

            _isProcessing = false;
            BoardReady?.Invoke();
        }

        private void InitializeSystems()
        {
            _validator = new BoardValidator(_config.BoardWidth, _config.BoardHeight, _positionConverter);
            _generator = new BoardGenerator(_config, _tileFactory, _tiles);
            _fallSystem = new BoardFallSystem(_config, _positionConverter, _tiles);
            _spawnSystem = new BoardSpawnSystem(_config, _tiles, _generator);

            _matchProcessor = new BoardMatchProcessor(
                _matchDetector,
                _tiles,
                _tileFactory,
                _fallSystem,
                _spawnSystem,
                UnsubscribeFromTile,
                UnsubscribeFromTile,
                _audioSystem,
                _scoreSystem
            );

            _matchProcessor.MatchesFound += matches => MatchesFound?.Invoke(matches);
        }

        private void SubscribeToAllTiles()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var tile = _tiles[x, y] as BasicTile;
                    if (tile != null)
                    {
                        SubscribeToTile(tile);
                    }
                }
            }
        }

        public bool CanSwap(Vector2Int pos1, Vector2Int pos2)
        {
            if (_validator == null)
                return false;

            return _validator.CanSwap(_tiles, pos1, pos2, _isProcessing);
        }

        public BoardValidator GetValidator()
        {
            return _validator;
        }

        public async UniTask<bool> TrySwapAsync(Vector2Int pos1, Vector2Int pos2)
        {
            if (!CanSwap(pos1, pos2))
            {
                SwapCompleted?.Invoke(false);
                return false;
            }

            _isProcessing = true;

            var tile1 = _tiles[pos1.x, pos1.y] as BasicTile;
            var tile2 = _tiles[pos2.x, pos2.y] as BasicTile;

            if (tile1 == null || tile2 == null)
            {
                Debug.LogError($"[Board] Tiles are null: tile1={tile1}, tile2={tile2}");
                _isProcessing = false;
                SwapCompleted?.Invoke(false);
                return false;
            }

            // Check if swap would create matches
            bool wouldCreateMatch = _matchDetector.WouldCreateMatch(_tiles, pos1, pos2);

            if (!wouldCreateMatch)
            {
                Debug.Log("[Board] Swap would not create match");
                _audioSystem.PlayErrorEffect();
                _isProcessing = false;
                SwapCompleted?.Invoke(false);
                return false;
            }

            await PerformSwapAsync(tile1, tile2, pos1, pos2);

            // Process matches
            await _matchProcessor.ProcessMatchesAsync();
            SubscribeToAllTiles();
            TilesSpawned?.Invoke();

            _isProcessing = false;
            SwapCompleted?.Invoke(true);
            Debug.Log("[Board] Swap completed successfully");
            return true;
        }

        private async UniTask PerformSwapAsync(BasicTile tile1, BasicTile tile2, Vector2Int pos1, Vector2Int pos2)
        {
            Vector2 anchoredPos1 = _positionConverter.GridToAnchoredPosition(pos1);
            Vector2 anchoredPos2 = _positionConverter.GridToAnchoredPosition(pos2);

            // Swap in array
            _tiles[pos1.x, pos1.y] = tile2;
            _tiles[pos2.x, pos2.y] = tile1;

            // Update positions
            tile1.SetPosition(pos2);
            tile2.SetPosition(pos1);

            await UniTask.WhenAll(
                tile1.AnimateSwapAsync(anchoredPos2, _config.SwapDuration),
                tile2.AnimateSwapAsync(anchoredPos1, _config.SwapDuration)
            );
        }

        public TileBase GetTileAt(Vector2Int position)
        {
            if (!_positionConverter.IsValidGridPosition(position))
                return null;

            return _tiles[position.x, position.y];
        }

        public Vector2Int? GetGridPosition(Vector3 worldPosition)
        {
            Vector2Int gridPos = _positionConverter.WorldToGridPosition(worldPosition);
            return _positionConverter.IsValidGridPosition(gridPos) ? gridPos : (Vector2Int?)null;
        }

        public TileBase[,] GetTiles()
        {
            return _tiles;
        }

        public void SetTileFactory(ITileFactory tileFactory)
        {
            _tileFactory = tileFactory;
        }

        public BoardConfig GetConfig()
        {
            return _config;
        }

        public BasicTile GetTilePrefab()
        {
            return _tilePrefab;
        }

        public Transform GetTilesParent()
        {
            return _tilesParent;
        }

        public BoardPositionConverter GetPositionConverter()
        {
            return _positionConverter;
        }

        public void SetPositionConverter(BoardPositionConverter positionConverter)
        {
            _positionConverter = positionConverter;
            _validator = new BoardValidator(_config.BoardWidth, _config.BoardHeight, _positionConverter);
            if (_fallSystem != null)
            {
                _fallSystem = new BoardFallSystem(_config, _positionConverter, _tiles);
            }
        }
    }
}