using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MiniIT.TILES
{
    [RequireComponent(typeof(Image))]
    public class TileView : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private float _selectedScale = 1.1f;
        [SerializeField] private float _destroyDuration = 0.3f;
        
        private Vector3 _originalScale;
        private Tween _currentTween;
        private RectTransform _rectTransform;
        
        private void Awake()
        {
            if (_image == null)
                _image = GetComponent<Image>();
            
            _rectTransform = GetComponent<RectTransform>();
            _originalScale = _rectTransform.localScale;
        }
        
        private void OnDestroy()
        {
            _currentTween?.Kill();
        }
        
        public void SetSprite(Sprite sprite)
        {
            if (_image != null && sprite != null)
            {
                _image.sprite = sprite;
            }
        }
        
        public void SetSelected(bool selected)
        {
            _currentTween?.Kill();
            
            Vector3 targetScale = selected ? _originalScale * _selectedScale : _originalScale;
            _currentTween = _rectTransform.DOScale(targetScale, 0.15f)
                .SetEase(Ease.OutBack);
        }
        
        public async UniTask AnimateSwapAsync(Vector2 targetAnchoredPosition, float duration)
        {
            _currentTween?.Kill();
            
            _currentTween = _rectTransform.DOAnchorPos(targetAnchoredPosition, duration)
                .SetEase(Ease.OutQuad);
            
            await UniTask.WaitUntil(() => !_currentTween.IsActive());
            _currentTween = null;
        }
        
        public async UniTask AnimateFallAsync(Vector2 targetAnchoredPosition, float duration)
        {
            _currentTween?.Kill();
            
            _currentTween = _rectTransform.DOAnchorPos(targetAnchoredPosition, duration)
                .SetEase(Ease.OutQuad);
            
            await UniTask.WaitUntil(() => !_currentTween.IsActive());
            _currentTween = null;
        }
        
        public async UniTask AnimateDestroyAsync()
        {
            _currentTween?.Kill();
            
            Sequence destroySequence = DOTween.Sequence();
            destroySequence.Append(_rectTransform.DOScale(Vector3.zero, _destroyDuration * 0.7f).SetEase(Ease.InBack));
            destroySequence.Join(_image.DOFade(0f, _destroyDuration).SetEase(Ease.InQuad));
            
            _currentTween = destroySequence;
            await UniTask.WaitUntil(() => !_currentTween.IsActive());
            _currentTween = null;
        }
        
        public async UniTask AnimateSpawnAsync(float duration)
        {
            _rectTransform.localScale = Vector3.zero;
            Color color = _image.color;
            color.a = 0f;
            _image.color = color;
            
            _currentTween?.Kill();
            
            Sequence spawnSequence = DOTween.Sequence();
            spawnSequence.Append(_rectTransform.DOScale(_originalScale, duration).SetEase(Ease.OutBack));
            spawnSequence.Join(_image.DOFade(1f, duration).SetEase(Ease.OutQuad));
            
            _currentTween = spawnSequence;
            await UniTask.WaitUntil(() => !_currentTween.IsActive());
            _currentTween = null;
        }
        
        public void ResetVisuals()
        {
            _currentTween?.Kill();
            _rectTransform.localScale = _originalScale;
            Color color = _image.color;
            color.a = 1f;
            _image.color = color;
        }
    }
}