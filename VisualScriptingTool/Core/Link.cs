using System;

namespace NodeEditor
{
    [Serializable]
    public class Link
    {
        public int NodeId = Node.NoNodeID;

        [NonSerialized]
        public ValueType Type;
        [NonSerialized]
        public string Name;
        [NonSerialized]
        public Settings ValueSettings;

        const Settings ValueRequiredNegative = (Settings)(-1) ^ Settings.ValueRequired;
        public bool ValueRequired
        {
            get {return (ValueSettings & Settings.ValueRequired) != 0;}
            set
            {
                if (value) ValueSettings |= Settings.ValueRequired;
                else ValueSettings &= ValueRequiredNegative;
            }
        }
        const Settings NoDefaultNegative = (Settings)(-1) ^ Settings.NoDefaults;
        public bool NoDefaults
        {
            get {return (ValueSettings & Settings.NoDefaults) != 0;}
            set
            {
                if (value) ValueSettings |= Settings.NoDefaults;
                else ValueSettings &= NoDefaultNegative;
            }
        }

        public bool IsConnected{get {return NodeId > Node.NoNodeID;}}

        [NonSerialized]
        public int LastDefaultNode = Node.NoNodeID;

        public void Initialize(ValueType type, string name, Settings valueSettings = Settings.ValueRequired)
        {
            Type = type;
            Name = name;
            ValueSettings = valueSettings;
        }

        [Flags]
        public enum Settings
        {
            None = 0,
            ValueRequired = 1,
            NoDefaults = 1 << 1,
        }
    }
}