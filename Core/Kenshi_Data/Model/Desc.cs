using Core.Kenshi_Data.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Kenshi_Data.Model
{
    public class Desc
    {
        public ItemType list = ItemType.NULL_ITEM;
        public string description = "";
        public string category = "misc";
        public string mask = "";
        public object defaultValue;
        public int flags;
        public int limit;
        public DescCondition condition;
        public class DescCondition
        {
            public string key;
            public object values;
            public bool match;
        }
    }
}
