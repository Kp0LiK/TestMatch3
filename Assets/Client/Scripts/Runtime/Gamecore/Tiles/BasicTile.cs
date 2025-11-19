using System;
using Cysharp.Threading.Tasks;
using MiniIT.BOARD;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniIT.TILES
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(TileView))]
    public class BasicTile : TileBase, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public event Action<BasicTile> TileSelected;
        public event Action<BasicTile> TileDeselected;

        private bool _isDragging;
        private Vector3 _dragStartPosition;

        public override void Init(int colorId, Vector2Int position, Sprite sprite)
        {
            base.Init(colorId, position, sprite);
            view?.ResetVisuals();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!CanInteract()) return;

            _isDragging = false;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                RectTransform, 
                eventData.position, 
                eventData.pressEventCamera, 
                out Vector2 localPoint);
            _dragStartPosition = localPoint;
            SetSelected(true);
            TileSelected?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!CanInteract()) return;

            _isDragging = true;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                RectTransform, 
                eventData.position, 
                eventData.pressEventCamera, 
                out Vector2 localPoint);
            Vector3 currentPosition = localPoint;
            Vector3 dragDelta = currentPosition - _dragStartPosition;

            if (dragDelta.magnitude > 0.3f)
            {
                GetDragDirection(dragDelta);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!CanInteract()) return;

            if (_isDragging)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    RectTransform, 
                    eventData.position, 
                    eventData.pressEventCamera, 
                    out Vector2 localPoint);
                Vector3 endPosition = localPoint;
                Vector3 dragDelta = endPosition - _dragStartPosition;

                if (dragDelta.magnitude > 0.3f)
                {
                    Vector2Int direction = GetDragDirection(dragDelta);
                    if (direction != Vector2Int.zero)
                    {
                        // Try to swap in direction
                        TrySwapInDirection(direction);
                    }
                }
            }

            SetSelected(false);
            TileDeselected?.Invoke(this);
        }
        
        private async void TrySwapInDirection(Vector2Int direction)
        {
            if (State != TileState.Selected)
                return;
                
            Vector2Int currentPos = Position;
            Vector2Int targetPos = currentPos + direction;
            
            // Get Board through parent or find it
            Board board = GetComponentInParent<Board>();
            if (board == null)
            {
                Debug.LogError("[BasicTile] Board not found in parent hierarchy");
                return;
            }
            
            if (!board.CanSwap(currentPos, targetPos))
            {
                Debug.Log($"[BasicTile] Cannot swap from {currentPos} to {targetPos}");
                return;
            }
            
            bool success = await board.TrySwapAsync(currentPos, targetPos);
            if (!success)
            {
                Debug.Log($"[BasicTile] Swap failed from {currentPos} to {targetPos}");
            }
        }

        private Vector2Int GetDragDirection(Vector3 delta)
        {
            float absX = Mathf.Abs(delta.x);
            float absY = Mathf.Abs(delta.y);

            if (absX > absY)
            {
                return delta.x > 0 ? Vector2Int.right : Vector2Int.left;
            }
            else
            {
                return delta.y > 0 ? Vector2Int.up : Vector2Int.down;
            }
        }

        private void SetSelected(bool selected)
        {
            if (selected && State == TileState.Idle)
            {
                SetState(TileState.Selected);
                view?.SetSelected(true);
            }
            else if (!selected && State == TileState.Selected)
            {
                SetState(TileState.Idle);
                view?.SetSelected(false);
            }
        }

        public async UniTask AnimateSwapAsync(Vector2 targetAnchoredPos, float duration)
        {
            if (view == null) return;

            SetState(TileState.Swapping);
            await view.AnimateSwapAsync(targetAnchoredPos, duration);

            if (State == TileState.Swapping)
            {
                SetState(TileState.Idle);
            }
        }

        public async UniTask AnimateFallAsync(Vector2 targetAnchoredPos, float duration)
        {
            if (view == null) return;

            SetState(TileState.Falling);
            await view.AnimateFallAsync(targetAnchoredPos, duration);

            if (State == TileState.Falling)
            {
                SetState(TileState.Idle);
            }
        }

        public async UniTask AnimateDestroyAsync()
        {
            if (view == null) return;

            SetState(TileState.Destroying);
            await view.AnimateDestroyAsync();
            SetState(TileState.Destroyed);
        }

        public async UniTask AnimateSpawnAsync(float duration)
        {
            if (view == null) return;

            await view.AnimateSpawnAsync(duration);
            SetState(TileState.Idle);
        }
    }
}