using System;
using UnityEngine;

namespace MiniIT.TILES
{
    public enum TileState
    {
        Idle,
        Selected,
        Swapping,
        Falling,
        Destroying,
        Destroyed
    }

    public abstract class TileBase : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] protected TileView view;
        
        public int ColorId { get; protected set; }
        public Vector2Int Position { get; private set; }
        public TileState State { get; protected set; }

        public RectTransform RectTransform => _rectTransform;

        public event Action<TileBase> StateChanged;
        public event Action<TileBase> Destroyed;
        
        protected virtual void Awake()
        {
            if (view == null)
                view = GetComponent<TileView>();
        }
        
        public virtual void Init(int colorId, Vector2Int position, Sprite sprite)
        {
            ColorId = colorId;
            Position = position;
            SetState(TileState.Idle);
            view?.SetSprite(sprite);
        }
        
        public void SetPosition(Vector2Int newPos)
        {
            Position = newPos;
        }

        protected void SetState(TileState newState)
        {
            if (State == newState) return;
            
            State = newState;
            StateChanged?.Invoke(this);
            
            if (State == TileState.Destroyed)
            {
                Destroyed?.Invoke(this);
            }
        }
        
        public bool CanInteract()
        {
            return State == TileState.Idle || State == TileState.Selected;
        }
        
        public bool IsMatchable()
        {
            return State != TileState.Destroyed && State != TileState.Destroying;
        }
    }
}