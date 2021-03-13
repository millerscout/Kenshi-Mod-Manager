using Core.Kenshi_Data.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.LocalData.Models
{
    public ref struct ModChange
    {
        public ModChange(int id, int modId, ItemType type, string section, string key, string oldVal, string newVal)
        {
            Id = id;
            ModId = modId;
            Type = type;
            Section = section;
            Key = key;
            OldVal = oldVal;
            NewVal = newVal;
        }

        public int Id { get; set; }
        public int ModId { get; set; }
        public ItemType Type { get; set; }
        public string Section { get; set; }
        public string Key { get; set; }
        public string OldVal { get; set; }
        public string NewVal { get; set; }
    }
}
