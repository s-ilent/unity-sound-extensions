// FILE: SoundRegistry.cs
using UnityEngine;
using System.Collections.Generic;

namespace Silent.Audio
{
    [CreateAssetMenu(fileName = "SoundRegistry", menuName = "Audio/Sound Registry")]
    public class SoundRegistry : ScriptableObject
    {
        public List<AudioCue> audioCues;
    }
}
