using Core.Kenshi_Data.Enums;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace Core
{
    public class Changes
    {
        public List<Changes> Children { get; set; }

        public int Level { get; set; }
        public Changes Parent { get; private set; }

        public void Add(Changes n)
        {
            this.Children.Add(n);
            n.Parent = this;
        }

        public int getVisibleChildNodeCount()
        {
            int num = 0;
            if (this.Children != null)
            {
                foreach (ChangeData child in this.Children)
                    num += 1 + child.getVisibleChildNodeCount();
            }
            return num;
        }

        public Changes()
        {
            this.Level = 0;
        }
    }
    public class ChangeData : Changes
    {
        public ChangeType Type { get; }

        public object OldValue { get; }

        public object NewValue { get; }

        public string Text { get; }

        public string Key { get; }

        public Color Colour { get; }

        public string Section { get; }

        public ChangeData(
          ChangeType type,
          string section,
          string key,
          object oldVal,
          object newVal,
          State state)
        {
            this.Type = type;
            this.OldValue = oldVal;
            this.NewValue = newVal;
            this.Key = key;
            this.Section = section;
            this.Colour = GetStateColor(state);
        }

        public ChangeData(ChangeType type, string key, State state, string text)
        {
            this.Type = type;
            this.Key = key;
            this.Colour = GetStateColor(state);
            this.Text = text;
        }

        public override string ToString()
        {
            return this.OldValue != null && this.NewValue != null ? $"[{this.OldValue}] => [{this.NewValue}]" : (this.NewValue != null ? $"[{this.NewValue}]" : "");
        }

        public static Color GetStateColor(State state)
        {
            switch (state)
            {
                case State.UNKNOWN:
                    return Color.Red;

                case State.INVALID:
                    return Color.Red;

                case State.ORIGINAL:
                    return Color.Black;

                case State.OWNED:
                    return Color.Green;

                case State.MODIFIED:
                    return Color.Blue;

                case State.LOCKED:
                    return Color.DarkOrange;

                case State.REMOVED:
                    return Color.LightGray;

                case State.LOCKED_REMOVED:
                    return Color.LightGray;

                default:
                    return Color.Black;
            }
        }
    }

    public class EnumValue
    {
        public EnumDictionary type;

        public EnumDictionary Enum
        {
            get
            {
                return this.type;
            }
        }

        public int Value { get; set; }

        public string String
        {
            get
            {
                return this.type.Name(this.Value);
            }
            set
            {
                this.Value = this.type.Parse(value);
            }
        }

        public EnumValue(EnumDictionary e, int value)
        {
            this.type = e;
            this.Value = value;
        }

        public EnumValue(EnumDictionary e, string value)
        {
            this.type = e;
            this.String = value;
        }

        public override string ToString()
        {
            return this.String;
        }
    }

    public class EnumDictionary
    {
        public Dictionary<string, int> values = new Dictionary<string, int>();
        public int maxValue;

        public int Parse(string s)
        {
            return this.values.ContainsKey(s) ? this.values[s] : -1;
        }

        public string Name(int i)
        {
            foreach (KeyValuePair<string, int> keyValuePair in this.values)
            {
                if (keyValuePair.Value == i)
                    return keyValuePair.Key;
            }
            return (string)null;
        }
    }
}