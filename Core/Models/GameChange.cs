using Core.Kenshi_Data.Enums;

namespace Core.Models
{
    public readonly struct GameChange
    {
        public GameChange(State state, string modName, object value)
        {
            State = state;
            ModName = modName;
            Value = value;
        }

        public State State { get; }
        public string ModName { get; }
        public object Value { get; }
    }
}