using UnityEngine;

namespace MiniIT.BOARD
{
    [RequireComponent(typeof(RectTransform))]
    public class BoardView : MonoBehaviour
    {
        [SerializeField] private float _spacing = 4f;

        private RectTransform _boardRect;
        private Board _board;
        private BoardPositionConverter _positionConverter;

        public RectTransform BoardRect => _boardRect;
        public float Spacing => _spacing;

        private void Awake()
        {
            _boardRect = GetComponent<RectTransform>();
            _board = GetComponent<Board>();
        }

        public void InitializePositioning()
        {
            if (_board == null || _boardRect == null)
                return;

            float boardWidth = _boardRect.rect.width;
            float boardHeight = _boardRect.rect.height;

            float cellSize = Mathf.Min(
                (boardWidth - _spacing * (_board.Width - 1)) / _board.Width,
                (boardHeight - _spacing * (_board.Height - 1)) / _board.Height
            );

            _positionConverter = new BoardPositionConverter(
                _board.Width,
                _board.Height,
                cellSize,
                boardWidth,
                boardHeight,
                _spacing);

            _board.SetPositionConverter(_positionConverter);
        }

        //Helper method
        public void SetTileSize(RectTransform tileRect)
        {
            if (_positionConverter == null || tileRect == null)
                return;

            float cellSize = _positionConverter.GetCellSize();
            tileRect.sizeDelta = new Vector2(cellSize, cellSize);
        }

        //Helper method
        public float GetCellSize()
        {
            return _positionConverter?.GetCellSize() ?? _board.GetConfig()?.CellSize ?? 100f;
        }
    }
}