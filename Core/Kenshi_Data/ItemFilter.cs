using Core.Kenshi_Data.Enums;
using Core.Kenshi_Data.Model;
using System;
using System.Collections.Generic;

namespace Core
{
    public class ItemFilter
    {
        public ItemType type = ItemType.NULL_ITEM;
        public string name = "";
        public List<ItemFilter.PropertyFilter> property = new List<ItemFilter.PropertyFilter>();

        public bool Test(GameData.Item item)
        {
            if (this.type != ItemType.NULL_ITEM && item.type != this.type || this.name != "" && !item.Name.ToLower().Contains(this.name) && !item.stringID.ToLower().Contains(this.name))
                return false;
            foreach (ItemFilter.PropertyFilter propertyFilter in this.property)
            {
                object lvalue;
                if (propertyFilter.property == "Type")
                    lvalue = (object)item.type;
                else if (propertyFilter.property == "StringID")
                {
                    lvalue = (object)item.stringID;
                }
                else
                {
                    if (item.ContainsKey(propertyFilter.property + "0"))
                    {
                        bool flag = false;
                        int num = 0;
                        while (!flag)
                        {
                            string key = propertyFilter.property + (object)num;
                            if (item.ContainsKey(key))
                            {
                                flag |= this.compareValue(item[key], propertyFilter.value, propertyFilter.mode);
                                ++num;
                            }
                            else
                                break;
                        }
                        if (!flag)
                            return false;
                        continue;
                    }
                    if (item.ContainsKey(propertyFilter.property))
                    {
                        lvalue = item[propertyFilter.property];
                        if (lvalue is int && propertyFilter.mode <= PropertyFilter.Mode.NOT)
                        {
                            Desc desc = GameData.getDesc(item.type, propertyFilter.property);
                            if (desc != GameData.nullDesc)
                            {
                                Type type = desc.defaultValue.GetType();
                                if (type.IsEnum)
                                    lvalue = (object)Enum.GetName(type, lvalue);
                                if (lvalue == null)
                                    return false;
                            }
                        }
                    }
                    else if (item.hasReference(propertyFilter.property))
                    {
                        lvalue = (object)item.GetReferenceCount(propertyFilter.property);
                    }
                    else
                    {
                        Desc desc = GameData.getDesc(item.type, propertyFilter.property);
                        if (desc == null || desc.list == ItemType.NULL_ITEM)
                            return false;
                        lvalue = (object)0;
                    }
                }
                if (!this.compareValue(lvalue, propertyFilter.value, propertyFilter.mode))
                    return false;
            }
            return true;
        }

        public bool compareValue(
          object lvalue,
          object rvalue,
          ItemFilter.PropertyFilter.Mode comparison)
        {
            switch (comparison)
            {
                case PropertyFilter.Mode.EQUAL:
                    if (!lvalue.ToString().Equals(rvalue.ToString(), StringComparison.CurrentCultureIgnoreCase))
                        return false;
                    break;

                case PropertyFilter.Mode.CONTAINS:
                    if (!(lvalue.ToString().IndexOf(rvalue.ToString(), StringComparison.CurrentCultureIgnoreCase) >= 0))
                        return false;
                    break;

                case PropertyFilter.Mode.NOT:
                    if (lvalue.ToString().Equals(rvalue.ToString(), StringComparison.CurrentCultureIgnoreCase))
                        return false;
                    break;

                case PropertyFilter.Mode.GREATER:
                    float v1;
                    if (this.numberValue(lvalue, out v1) && (double)v1 <= (double)(float)rvalue)
                        return false;
                    break;

                case PropertyFilter.Mode.LESS:
                    float v2;
                    if (this.numberValue(lvalue, out v2) && (double)v2 >= (double)(float)rvalue)
                        return false;
                    break;

                case PropertyFilter.Mode.GEQUAL:
                    float v3;
                    if (this.numberValue(lvalue, out v3) && (double)v3 < (double)(float)rvalue)
                        return false;
                    break;

                case PropertyFilter.Mode.LEQUAL:
                    float v4;
                    if (this.numberValue(lvalue, out v4) && (double)v4 > (double)(float)rvalue)
                        return false;
                    break;
            }
            return true;
        }

        public bool numberValue(object o, out float v)
        {
            switch (o)
            {
                case int num:
                    v = (float)num;
                    break;

                case float num:
                    v = num;
                    break;

                default:
                    v = 0.0f;
                    return false;
            }
            return true;
        }

        public struct PropertyFilter
        {
            public ItemFilter.PropertyFilter.Mode mode;
            public string property;
            public object value;

            public enum Mode
            {
                EQUAL,
                CONTAINS,
                NOT,
                GREATER,
                LESS,
                GEQUAL,
                LEQUAL,
            }
        }
    }
}