// FILE: Editor/SoundRegistryEditor.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Silent.Audio
{
public class SoundRegistryEditor
{
    [MenuItem("Tools/Audio/Update Sound Registry")]
    private static void UpdateSoundRegistry()
    {
        string[] registryGuids = AssetDatabase.FindAssets("t:SoundRegistry");
        if (registryGuids.Length == 0) { Debug.LogError("No SoundRegistry asset found."); return; }

        string registryPath = AssetDatabase.GUIDToAssetPath(registryGuids[0]);
        SoundRegistry registry = AssetDatabase.LoadAssetAtPath<SoundRegistry>(registryPath);

        string[] cueGuids = AssetDatabase.FindAssets("t:AudioCue");
        registry.audioCues.Clear();

        foreach (string cueGuid in cueGuids)
        {
            string cuePath = AssetDatabase.GUIDToAssetPath(cueGuid);
            AudioCue cue = AssetDatabase.LoadAssetAtPath<AudioCue>(cuePath);
            registry.audioCues.Add(cue);
        }

        EditorUtility.SetDirty(registry);
        AssetDatabase.SaveAssets();
        Debug.Log($"SoundRegistry updated with {registry.audioCues.Count} AudioCues.");
    }
}
}
#endif
