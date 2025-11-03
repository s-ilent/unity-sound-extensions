// FILE: AudioCategory.cs
using UnityEngine;
using UnityEngine.Audio;

namespace Silent.Audio
{
    [CreateAssetMenu(fileName = "AudioCategory_", menuName = "Audio/Audio Category")]
    public class AudioCategory : ScriptableObject
    {
        public AudioMixerGroup mixerGroup;
        [Range(0, 256)] public int priority = 128;
    }
}
