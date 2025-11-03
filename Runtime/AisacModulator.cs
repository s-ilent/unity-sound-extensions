// FILE: AisacModulator.cs
using UnityEngine;
#if STEAMAUDIO_ENABLED
using SteamAudio;
#endif

namespace Silent.Audio
{
    public enum AisacTargetParameter
    {
        Volume, Pitch, LowPassCutoff, HighPassCutoff
#if STEAMAUDIO_ENABLED
        , Steam_DipoleWeight, Steam_DipolePower, Steam_OcclusionRadius
#endif
    }

    [System.Serializable]
    public class AisacModulator
    {
        public AisacControl control;
        public AisacTargetParameter target;
        public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
    }

}
