// FILE: SoundManager.cs
using UnityEngine;
using System.Collections.Generic;
using R3;
using Cysharp.Threading.Tasks;
#if STEAMAUDIO_ENABLED
using SteamAudio;
#endif

namespace Silent.Audio
{
    public class SoundManager : MonoBehaviour
    {
        // Helper class to hold cached component references for performance.
        private class PooledSource
        {
            public AudioSource Source;
            public AudioLowPassFilter LowPassFilter;
            public AudioHighPassFilter HighPassFilter;
#if STEAMAUDIO_ENABLED
            public SteamAudioSource SteamAudioSource;
#endif

            public PooledSource(GameObject sourceGO)
            {
                Source = sourceGO.GetComponent<AudioSource>();
                LowPassFilter = sourceGO.GetComponent<AudioLowPassFilter>();
                HighPassFilter = sourceGO.GetComponent<AudioHighPassFilter>();
#if STEAMAUDIO_ENABLED
                SteamAudioSource = sourceGO.GetComponent<SteamAudioSource>();
#endif
            }
        }

        // Helper class to track a playing sound and its properties for voice stealing.
        private class ActiveVoice
        {
            public PooledSource PooledSource;
            public int Priority;
        }

        [Header("Configuration")]
        [SerializeField] private SoundRegistry soundRegistry;

        [Header("Pooling")]
        [SerializeField] private int worldSpacePoolSize = 15;
        [SerializeField] private int uiSpacePoolSize = 5;

        private Queue<PooledSource> _worldSpacePool;
        private Queue<PooledSource> _uiSpacePool;
        private List<ActiveVoice> _activeVoices = new();
        private Dictionary<SerializableGuid, AudioCue> _cueLookup;

        void Awake()
        {
            // Build the fast lookup dictionary and warm up audio clips.
            _cueLookup = new Dictionary<SerializableGuid, AudioCue>();
            foreach (var cue in soundRegistry.audioCues)
            {
                if (!_cueLookup.ContainsKey(cue.UID))
                {
                    _cueLookup.Add(cue.UID, cue);
                    foreach (var clip in cue.clips) { if (clip != null) clip.LoadAudioData(); }
                }
            }

            // Initialize the object pools.
            _worldSpacePool = new Queue<PooledSource>();
            for (int i = 0; i < worldSpacePoolSize; i++) _worldSpacePool.Enqueue(CreatePooledSource($"WorldAudioSource_{i}", 1.0f));

            _uiSpacePool = new Queue<PooledSource>();
            for (int i = 0; i < uiSpacePoolSize; i++) _uiSpacePool.Enqueue(CreatePooledSource($"UIAudioSource_{i}", 0.0f));
        }

        private PooledSource CreatePooledSource(string name, float spatialBlend)
        {
            var sourceGO = new GameObject(name);
            sourceGO.transform.SetParent(this.transform);
            sourceGO.AddComponent<AudioSource>();
            sourceGO.AddComponent<AudioLowPassFilter>();
            sourceGO.AddComponent<AudioHighPassFilter>();
#if STEAMAUDIO_ENABLED
            sourceGO.AddComponent<SteamAudioSource>();
#endif

            var pooledSource = new PooledSource(sourceGO);
            pooledSource.Source.playOnAwake = false;
            pooledSource.Source.spatialBlend = spatialBlend;
            sourceGO.SetActive(false);
            return pooledSource;
        }

        void Start()
        {
            AudioEvents.OnSFXPlay.Subscribe(HandleSFXPlay).AddTo(this);
            // TODO: Add subscriptions for looping sounds when that feature is implemented.
        }

        private void HandleSFXPlay(SFXPlayRequest request)
        {
            if (_cueLookup.TryGetValue(request.CueUID, out AudioCue cue))
            {
                if (cue.clips == null || cue.clips.Length == 0) return;
                PlaySoundAsync(cue, request).Forget();
            }
        }

        private PooledSource TryGetAvailableSource(AudioCue cue)
        {
            var pool = cue.type == SFXType.WorldSpace ? _worldSpacePool : _uiSpacePool;
            if (pool.Count > 0) return pool.Dequeue();

            ActiveVoice voiceToSteal = null;
            int lowestPriority = int.MaxValue;
            foreach (var voice in _activeVoices)
            {
                if (voice.Priority < lowestPriority)
                {
                    lowestPriority = voice.Priority;
                    voiceToSteal = voice;
                }
            }

            int newPriority = cue.category?.priority ?? 128;
            if (voiceToSteal != null && newPriority > lowestPriority)
            {
                Debug.Log($"Stealing voice from sound with priority {lowestPriority} for new sound with priority {newPriority}");
                voiceToSteal.PooledSource.Source.Stop();
                _activeVoices.Remove(voiceToSteal);
                return voiceToSteal.PooledSource;
            }

            Debug.LogWarning("SoundManager pool depleted and no voice available to steal.");
            return null;
        }

        private async UniTaskVoid PlaySoundAsync(AudioCue cue, SFXPlayRequest request)
        {
            PooledSource pooledSource = TryGetAvailableSource(cue);
            if (pooledSource == null) return;

            // Get cached components
            AudioSource source = pooledSource.Source;
#if STEAMAUDIO_ENABLED
        SteamAudio.SteamAudioSource steamAudioSource = pooledSource.SteamAudioSource;
#endif
            AudioLowPassFilter lowPass = pooledSource.LowPassFilter;
            AudioHighPassFilter highPass = pooledSource.HighPassFilter;

            source.gameObject.SetActive(true);

            // --- Configure Standard Properties ---
            if (cue.type == SFXType.WorldSpace) source.transform.position = request.Position;
            if (cue.category != null) source.outputAudioMixerGroup = cue.category.mixerGroup;
            source.clip = cue.clips[Random.Range(0, cue.clips.Length)];

            // --- Prepare Final Parameter Values ---
            float finalVolume = cue.volume;
            float finalPitch = cue.pitch + Random.Range(-cue.pitchVariation, cue.pitchVariation);
            float finalLowPass = cue.lowPassCutoff;
            float finalHighPass = cue.highPassCutoff;
#if STEAMAUDIO_ENABLED
        float finalDipoleWeight = cue.dipoleWeight;
        float finalDipolePower = cue.dipolePower;
        float finalOcclusionRadius = cue.occlusionRadius;
#endif

            // --- Apply AISAC Modulation ---
            if (request.AisacValues != null)
            {
                foreach (var mod in cue.modulators)
                {
                    if (request.AisacValues.TryGetValue(mod.control, out float value))
                    {
                        float mappedValue = mod.curve.Evaluate(value);
                        switch (mod.target)
                        {
                            case AisacTargetParameter.Volume: finalVolume *= mappedValue; break;
                            case AisacTargetParameter.Pitch: finalPitch *= mappedValue; break;
                            case AisacTargetParameter.LowPassCutoff: finalLowPass = mappedValue; break;
                            case AisacTargetParameter.HighPassCutoff: finalHighPass = mappedValue; break;
#if STEAMAUDIO_ENABLED
                        case AisacTargetParameter.Steam_DipoleWeight: finalDipoleWeight = mappedValue; break;
                        case AisacTargetParameter.Steam_DipolePower: finalDipolePower = mappedValue; break;
                        case AisacTargetParameter.Steam_OcclusionRadius: finalOcclusionRadius = mappedValue; break;
#endif
                        }
                    }
                }
            }

            // --- Final Assignment to Components ---
            source.volume = finalVolume;
            source.pitch = finalPitch;

            lowPass.enabled = cue.useLowPassFilter;
            lowPass.cutoffFrequency = finalLowPass;
            highPass.enabled = cue.useHighPassFilter;
            highPass.cutoffFrequency = finalHighPass;

#if STEAMAUDIO_ENABLED
        steamAudioSource.airAbsorption = cue.useAirAbsorption;
        steamAudioSource.directivity = cue.useDirectivity;
        steamAudioSource.dipoleWeight = finalDipoleWeight;
        steamAudioSource.dipolePower = finalDipolePower;
        steamAudioSource.occlusion = cue.useOcclusion;
        steamAudioSource.occlusionType = cue.occlusionType;
        steamAudioSource.occlusionRadius = finalOcclusionRadius;
        steamAudioSource.occlusionSamples = cue.occlusionSamples;
        steamAudioSource.transmission = cue.useTransmission;
        steamAudioSource.transmissionType = cue.transmissionType;
#endif

            // --- Play and Manage Voice ---
            source.loop = cue.isLooping;
            source.Play();

            var activeVoice = new ActiveVoice { PooledSource = pooledSource, Priority = cue.category?.priority ?? 128 };
            _activeVoices.Add(activeVoice);

            float duration = (cue.isLooping && cue.loopDuration > 0)
                ? cue.loopDuration // It's a one-shot loop, use its defined duration.
                : source.clip.length; // It's a standard one-shot, use the clip's length.

            await UniTask.Delay(
                System.TimeSpan.FromSeconds(duration / source.pitch),
                ignoreTimeScale: true,
                cancellationToken: source.GetCancellationTokenOnDestroy()
            );

            // After the delay, if it was a one-shot loop, it needs to be stopped.
            if (source.isPlaying && source.loop)
            {
                source.Stop();
            }

            if (_activeVoices.Contains(activeVoice))
            {
                source.gameObject.SetActive(false);
                var pool = cue.type == SFXType.WorldSpace ? _worldSpacePool : _uiSpacePool;
                pool.Enqueue(pooledSource);
                _activeVoices.Remove(activeVoice);
            }
        }
    }
}
