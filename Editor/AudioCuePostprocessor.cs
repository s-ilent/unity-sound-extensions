// FILE: Editor/AudioCuePostprocessor.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Silent.Audio
{
/// <summary>
/// This class automatically runs whenever assets are changed in the project.
/// It ensures that all AudioCues have a unique ID and that the SoundRegistry is always up-to-date.
/// </summary>
public class AudioCuePostprocessor : AssetPostprocessor
{
    // This method is called by Unity with lists of all changed asset paths.
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        bool registryNeedsUpdate = false;

        // Check if any of our AudioCue assets were changed.
        foreach (string path in importedAssets.Concat(deletedAssets).Concat(movedAssets))
        {
            if (path.EndsWith(".asset") && AssetDatabase.LoadAssetAtPath<AudioCue>(path) != null)
            {
                registryNeedsUpdate = true;
                break;
            }
        }

        // If nothing relevant changed, we don't need to do anything.
        if (!registryNeedsUpdate) return;

        Debug.Log("AudioCue change detected. Validating UIDs and updating SoundRegistry...");

        // --- Step 1: Find and Fix any Duplicate or Empty UIDs ---
        // This is a robust way to handle asset duplication (Ctrl+D).
        var allCues = new List<AudioCue>();
        var seenUIDs = new HashSet<SerializableGuid>();

        // Find all AudioCue assets in the entire project.
        string[] allCueGuids = AssetDatabase.FindAssets("t:AudioCue");

        AssetDatabase.StartAssetEditing(); // Performance optimization for multiple asset edits
        try
        {
            foreach (string cueGuid in allCueGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(cueGuid);
                AudioCue cue = AssetDatabase.LoadAssetAtPath<AudioCue>(path);
                allCues.Add(cue);

                // If the UID is empty or we've seen it before, generate a new one.
                if (cue.UID.ToString() == null || !seenUIDs.Add(cue.UID))
                {
                    cue.GenerateNewUID();
                    EditorUtility.SetDirty(cue); // Mark the asset as changed
                    Debug.LogWarning($"Generated new UID for AudioCue at path: {path}", cue);
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing(); // Ensure this always runs
        }

        // --- Step 2: Update the Sound Registry ---
        string[] registryGuids = AssetDatabase.FindAssets("t:SoundRegistry");
        if (registryGuids.Length == 0) return; // No registry to update

        string registryPath = AssetDatabase.GUIDToAssetPath(registryGuids[0]);
        SoundRegistry registry = AssetDatabase.LoadAssetAtPath<SoundRegistry>(registryPath);

        // Check if the list is already up-to-date to avoid dirtying the asset unnecessarily.
        if (!registry.audioCues.SequenceEqual(allCues))
        {
            registry.audioCues.Clear();
            registry.audioCues.AddRange(allCues);
            EditorUtility.SetDirty(registry);
            Debug.Log($"SoundRegistry has been automatically updated with {allCues.Count} cues.");
        }
    }
}
}
#endif
