// FILE: GameEvents.cs
using UnityEngine;
using R3;
using System.Collections.Generic;

namespace Silent.Audio
{
    public enum SFXType { WorldSpace, UISpace }

    public struct SFXPlayRequest
    {
        public SerializableGuid CueUID;
        public Vector3 Position;
        public Dictionary<AisacControl, float> AisacValues;

        /// <summary>
        /// Constructor for 3D, world-space sounds.
        /// </summary>
        public SFXPlayRequest(AudioCue cue, Vector3 position, Dictionary<AisacControl, float> aisacValues = null)
        {
            CueUID = cue.UID;
            Position = position;
            AisacValues = aisacValues;
        }

        /// <summary>
        /// Constructor for 2D, UI-space sounds.
        /// </summary>
        public SFXPlayRequest(AudioCue cue, Dictionary<AisacControl, float> aisacValues = null)
        {
            CueUID = cue.UID;
            Position = Vector3.zero;
            AisacValues = aisacValues;
        }
    }

    public struct LoopingSFXStartRequest
    {
        public SerializableGuid CueUID;
        public object Owner; // The object starting the loop (e.g., a component instance)
    }

    public struct LoopingSFXStopRequest
    {
        public object Owner; // The object that previously started the loop
    }

    public static class AudioEvents
    {
        public static readonly Subject<SFXPlayRequest> OnSFXPlay = new();
        public static readonly Subject<LoopingSFXStartRequest> OnLoopingSFXStart = new();
        public static readonly Subject<LoopingSFXStopRequest> OnLoopingSFXStop = new();
    }
}
