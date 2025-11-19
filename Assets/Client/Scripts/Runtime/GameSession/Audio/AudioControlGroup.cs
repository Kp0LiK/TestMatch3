using System;
using Sirenix.OdinInspector;
using UnityEngine.UI;

namespace MiniIT.AUDIO
{
    [Serializable]
    public class AudioControlGroup
    {
        public AudioType Type;
        public Slider Slider;

        [InfoBox("ForExample, MusicVolume")] public string MixerParameter;

        [NonSerialized] public float LastValue;
    }
}