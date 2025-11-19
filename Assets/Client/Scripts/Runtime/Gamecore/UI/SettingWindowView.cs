using System.Collections.Generic;
using MiniIT.AUDIO;
using MiniIT.EXTENSION;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace MiniIT.UI
{
    public class SettingWindowView : MonoBehaviour, IWindow
    {
        [SerializeField, BoxGroup("Audio Groups")]
        private List<AudioControlGroup> _audioGroups = new();
        
        [SerializeField] private Button _closeButton;

        private AudioSystem _audioSystem;

        [Inject]
        public void Construct(AudioSystem audioSystem)
        {
            _audioSystem = audioSystem;
        }

        private void Awake()
        {
            Close();
        }

        private void OnEnable()
        {
            foreach (var group in _audioGroups)
            {
                var savedVolume = _audioSystem.GetVolume(group.Type);
                group.LastValue = savedVolume.Remap(-80f, 0f, 0f, 100f);
                group.Slider.SetValueWithoutNotify(group.LastValue);

                group.Slider.onValueChanged.AddListener(value => OnSliderChanged(group, value));
            }
            
            _closeButton.onClick.AddListener(Close);
        }

        private void OnDisable()
        {
            foreach (var group in _audioGroups)
            {
                group.Slider.onValueChanged.RemoveAllListeners();
            }
            
            _closeButton.onClick.RemoveListener(Close);
        }

        private void OnSliderChanged(AudioControlGroup group, float value)
        {
            group.LastValue = value;
            var mixerValue = value.Remap(0f, 100f, -80f, 0f);
            _audioSystem.SetVolume(group.Type, mixerValue);
        }

        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}