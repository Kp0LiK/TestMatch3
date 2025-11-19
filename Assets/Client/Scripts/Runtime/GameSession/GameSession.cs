using System;
using Cysharp.Threading.Tasks;
using MiniIT.AUDIO;
using MiniIT.BOARD;
using MiniIT.SCORE;
using MiniIT.TILES.FACTORY;
using UnityEngine;
using Zenject;

namespace MiniIT.GAMESESSION
{
    public class GameSession : MonoBehaviour
    {
        private Board _board;
        private ITileFactory _tileFactory;
        private AudioSystem _audioSystem;
        private ScoreSystem _scoreSystem;

        public AudioSystem Audio => _audioSystem;
        public ScoreSystem Score => _scoreSystem;

        public event Action GameStarted;

        [Inject]
        private void Construct(
            Board board,
            ITileFactory tileFactory,
            AudioSystem audioSystem,
            ScoreSystem scoreSystem)
        {
            _board = board;
            _tileFactory = tileFactory;
            _audioSystem = audioSystem;
            _scoreSystem = scoreSystem;
        }

        private async void Start()
        {
            await InitializeGameAsync();
        }

        private async UniTask InitializeGameAsync()
        {
            _board.SetTileFactory(_tileFactory);
            
            await _board.InitializeAsync();
            
            GameStarted?.Invoke();
        }

        private void OnDestroy()
        {
            _tileFactory?.ClearPool();
        }
    }
}

