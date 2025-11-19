using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MiniIT.AUDIO
{
    public class AudioSystem : MonoBehaviour
    {
        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private AudioSource _effectsSource;
        [SerializeField] private AudioClip[] _comboSounds;
        [SerializeField] private AudioClip _errorEffect;

        private const float OFF_SOUND = -80f;

        private readonly Dictionary<AudioType, string> _prefsKeys = new()
        {
            { AudioType.Music, "MusicVolume" },
            { AudioType.Effect, "EffectVolume" }
        };

        private void Start()
        {
            if (_mixer == null)
            {
                Debug.LogError("[AudioSystem] AudioMixer is not assigned!");
                return;
            }
            
            ApplyAllVolumes();
        }

        public float GetVolume(AudioType type)
        {
            if (!_prefsKeys.ContainsKey(type))
            {
                Debug.LogError($"[AudioSystem] Unknown audio type: {type}");
                return OFF_SOUND;
            }

            var key = _prefsKeys[type];
            var volume = PlayerPrefs.GetFloat(key, 0f);

            return volume;
        }

        public void SetVolume(AudioType type, float value)
        {
            if (!_prefsKeys.ContainsKey(type))
            {
                Debug.LogError($"[AudioSystem] Unknown audio type: {type}");
                return;
            }

            var key = _prefsKeys[type];
            PlayerPrefs.SetFloat(key, value);
            _mixer.SetFloat(key, value);
            PlayerPrefs.Save();
        }

        public void ApplyAllVolumes()
        {
            if (_mixer == null)
                return;

            foreach (var kvp in _prefsKeys)
            {
                var key = kvp.Value;
                var volume = PlayerPrefs.GetFloat(key, 0f); // Default to 0 (full volume)
                _mixer.SetFloat(key, volume);
            }

            Debug.Log("[AudioSystem] All volumes applied from PlayerPrefs");
        }

        public void PlayComboSound(int comboLevel)
        {
            int soundIndex = Mathf.Clamp(comboLevel - 1, 0, _comboSounds.Length - 1);

            if (soundIndex < 0 || soundIndex >= _comboSounds.Length)
            {
                return;
            }

            var clip = _comboSounds[soundIndex];

            _effectsSource.PlayOneShot(clip);
        }

        public void PlayErrorEffect()
        {
            _effectsSource.PlayOneShot(_errorEffect);
        }
    }
}