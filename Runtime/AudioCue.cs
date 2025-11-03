// FILE: AudioCue.cs
using UnityEngine;
#if STEAMAUDIO_ENABLED
using SteamAudio;
#endif

namespace Silent.Audio
{
    [CreateAssetMenu(fileName = "AudioCue_", menuName = "Audio/Audio Cue")]
    public class AudioCue : ScriptableObject
    {
        [Header("Configuration")]
        public SFXType type = SFXType.WorldSpace;
        public AudioCategory category;

        [Header("Sound")]
        public AudioClip[] clips;
        [Range(0f, 1f)] public float volume = 1.0f;
        [Range(0.1f, 3f)] public float pitch = 1.0f;
        [Range(0f, 1f)] public float pitchVariation = 0.0f;

        [Header("Looping")]
        [Tooltip("Is this a looping sound? If false, it will play once.")]
        public bool isLooping = false;

        [Tooltip("If > 0, the loop will automatically stop after this many seconds. If 0, it is a stateful loop that must be stopped manually.")]
        public float loopDuration = 0.0f;

        [Header("Filters")]
        [Tooltip("If checked, the Low-Pass filter will be active.")]
        public bool useLowPassFilter;
        [Tooltip("If checked, the High-Pass filter will be active.")]
        public bool useHighPassFilter;
        [Range(10f, 22000f)] public float lowPassCutoff = 22000f;
        [Range(10f, 22000f)] public float highPassCutoff = 10f;

#if STEAMAUDIO_ENABLED
        [Header("Steam Audio - Air Absorption")]
        public bool useAirAbsorption = false;

        [Header("Steam Audio - Directivity")]
        public bool useDirectivity = false;
        [Range(0.0f, 1.0f)] public float dipoleWeight = 0.0f;
        [Range(0.0f, 4.0f)] public float dipolePower = 0.0f;

        [Header("Steam Audio - Occlusion & Transmission")]
        public bool useOcclusion = false;
        public OcclusionType occlusionType = OcclusionType.Raycast;
        [Range(0.0f, 4.0f)] public float occlusionRadius = 1.0f;
        [Range(1, 128)] public int occlusionSamples = 16;
        [Space(5)]
        public bool useTransmission = false;
        public TransmissionType transmissionType = TransmissionType.FrequencyIndependent;
#endif

        [Header("AISAC")]
        public AisacModulator[] modulators;

        [Header("Identification")]
        [SerializeField] private SerializableGuid uid;
        public SerializableGuid UID => uid;

        public void GenerateNewUID()
        {
            uid = SerializableGuid.NewGuid();
        }

#if UNITY_EDITOR
#endif
    }
}
