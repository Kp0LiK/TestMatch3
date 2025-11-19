using MiniIT.AUDIO;
using Zenject;

namespace MiniIT.INSTALLERS
{
    /// <summary>
    /// Main project installer for global dependencies that persist across all scenes
    /// </summary>
    public class MainProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            BindAudioSystem();
        }

        private void BindAudioSystem()
        {
            Container.Bind<AudioSystem>()
                .FromComponentInNewPrefabResource("Audio/AudioSystem")
                .AsSingle()
                .NonLazy();
        }
    }
}