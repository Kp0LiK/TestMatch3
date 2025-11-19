using MiniIT.BOARD;
using MiniIT.SCORE;
using MiniIT.TILES.FACTORY;
using UnityEngine;
using Zenject;

namespace MiniIT.INSTALLERS
{
    /// <summary>
    /// Game scene installer for game-specific dependencies
    /// </summary>
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private BoardConfig _boardConfig;

        public override void InstallBindings()
        {
            BindBoard();
            BindTileFactory();
            BindSwapValidator();
            BindScoreSystem();
        }

        private void BindBoard()
        {
            Container.Bind<BoardConfig>().FromInstance(_boardConfig).AsSingle();

            Container.Bind<Board>().FromComponentInHierarchy().AsSingle();
        }

        private void BindTileFactory()
        {
            Container.Bind<ITileFactory>().FromMethod(CreateTileFactory).AsSingle();
        }

        private void BindSwapValidator()
        {
        }
        
        private void BindScoreSystem()
        {
            Container.Bind<ScoreSystem>().AsSingle();
        }

        private ITileFactory CreateTileFactory(InjectContext context)
        {
            var board = context.Container.Resolve<Board>();

            var tilePrefab = board.GetTilePrefab();
            var tilesParent = board.GetTilesParent();

            return new TileFactory(
                tilePrefab,
                tilesParent,
                context.Container,
                board,
                initialPoolSize: 20,
                maxPoolSize: 100
            );
        }
    }
}