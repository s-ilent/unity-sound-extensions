// FILE: SerializableGuid.cs
using UnityEngine;
using System;

namespace Silent.Audio
{
    [Serializable]
    public struct SerializableGuid : IEquatable<SerializableGuid>
    {
        [SerializeField]
        private string value;

        public Guid ToGuid() => new Guid(value);

        public SerializableGuid(Guid guid)
        {
            value = guid.ToString();
        }

        public static SerializableGuid NewGuid()
        {
            return new SerializableGuid(Guid.NewGuid());
        }

        public override string ToString() => value;
        public override bool Equals(object obj) => obj is SerializableGuid other && this.Equals(other);
        public bool Equals(SerializableGuid other) => value == other.value;
        public override int GetHashCode() => (value != null) ? value.GetHashCode() : 0;

        public static bool operator ==(SerializableGuid a, SerializableGuid b) => a.Equals(b);
        public static bool operator !=(SerializableGuid a, SerializableGuid b) => !a.Equals(b);
    }
}
