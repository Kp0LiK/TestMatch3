using DG.Tweening;
using MiniIT.SCORE;
using TMPro;
using UnityEngine;
using Zenject;

namespace MiniIT.UI
{
    public class ScoreView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;

        private ScoreSystem _scoreSystem;
        private int _displayedScore;
        private Tween _scoreTween;
        private Sequence _popupSequence;

        [Inject]
        public void Construct(ScoreSystem scoreSystem)
        {
            _scoreSystem = scoreSystem;
        }

        private void Awake()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            _scoreTween?.Kill();
            _popupSequence?.Kill();
        }

        public void Initialize()
        {
            _displayedScore = _scoreSystem.CurrentScore;
            UpdateScoreText(_displayedScore);

            _scoreSystem.ScoreChanged += OnScoreChanged;
        }

        private void OnDisable()
        {
            if (_scoreSystem != null)
            {
                _scoreSystem.ScoreChanged -= OnScoreChanged;
            }
        }

        private void OnScoreChanged(int newScore)
        {
            AnimateScore(newScore);
        }

        private void AnimateScore(int targetScore)
        {
            _scoreTween?.Kill();

            _scoreTween = DOTween.To(
                () => _displayedScore,
                value =>
                {
                    _displayedScore = value;
                    UpdateScoreText(_displayedScore);
                },
                targetScore,
                0.5f
            ).SetEase(Ease.OutQuad);
        }

        private void UpdateScoreText(int score)
        {
            _scoreText.text = $"Score: {score:N0}";
        }
    }
}