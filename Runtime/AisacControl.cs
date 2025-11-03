// FILE: AisacControl.cs
using UnityEngine;

namespace Silent.Audio
{
    [CreateAssetMenu(fileName = "Aisac_", menuName = "Audio/AISAC Control")]
    public class AisacControl : ScriptableObject
    {
        [SerializeField]
        public string description;
    }
}
