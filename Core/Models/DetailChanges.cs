using Core.Kenshi_Data.Enums;

namespace Core.Models
{
    public readonly struct DetailChanges
    {
        public DetailChanges(ItemType type, string name, string propertyKey)
        {
            Type = type;
            Name = name;
            PropertyKey = propertyKey;
        }

        public ItemType Type { get; }
        public string Name { get; }
        public string PropertyKey { get; }
    }
}