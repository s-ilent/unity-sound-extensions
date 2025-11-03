// FILE: AudioCueExtensions.cs
using UnityEngine;

namespace Silent.Audio
{
    /// <summary>
    /// Provides simple, clean extension methods for playing an AudioCue.
    /// This simplifies the API so gameplay programmers don't need to know about the event system.
    /// </summary>
    public static class AudioCueExtensions
    {
        /// <summary>
        /// Plays the AudioCue as a 3D sound at a specific world position.
        /// </summary>
        /// <param name="cue">The audio cue to play.</param>
        /// <param name="position">The position in world space to play the sound.</param>
        public static void Play(this AudioCue cue, Vector3 position)
        {
            if (cue == null) return;
            AudioEvents.OnSFXPlay.OnNext(new SFXPlayRequest(cue, position));
        }

        /// <summary>
        /// Plays the AudioCue as a 3D sound attached to a GameObject/Component.
        /// </summary>
        /// <param name="cue">The audio cue to play.</param>
        /// <param name="emitter">The component whose transform will be used as the sound's position.</param>
        public static void Play(this AudioCue cue, Component emitter)
        {
            if (cue == null || emitter == null) return;
            AudioEvents.OnSFXPlay.OnNext(new SFXPlayRequest(cue, emitter.transform.position));
        }

        /// <summary>
        /// Plays the AudioCue as a 2D, non-spatialized sound (for UI).
        /// </summary>
        /// <param name="cue">The audio cue to play.</param>
        public static void Play(this AudioCue cue)
        {
            if (cue == null) return;
            // The SFXPlayRequest constructor for 2D sounds handles the rest.
            AudioEvents.OnSFXPlay.OnNext(new SFXPlayRequest(cue));
        }

        /// <summary>
        /// Starts a stateful looping sound, tied to a specific owner object.
        /// The loop will continue until StopLoop is called.
        /// </summary>
        public static void StartLoop(this AudioCue cue, object owner)
        {
            if (cue == null || owner == null) return;
            var request = new LoopingSFXStartRequest { CueUID = cue.UID, Owner = owner };
            AudioEvents.OnLoopingSFXStart.OnNext(request);
        }

        /// <summary>
        /// Stops a stateful looping sound associated with the given owner.
        /// </summary>
        public static void StopLoop(this AudioCue cue, object owner)
        {
            if (cue == null || owner == null) return;
            // The request only needs the owner to find the active loop.
            var request = new LoopingSFXStopRequest { Owner = owner };
            AudioEvents.OnLoopingSFXStop.OnNext(request);
        }
    }
}
