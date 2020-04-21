using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class GameChanges
    {
        public string TypeOfChange { get; set; }
        public List<ItemGameChanges> Items { get; set; }
    }
    public class ItemGameChanges
    {
        public string Name { get; set; }
        public List<Tuple<string, string, object>> Changes { get; set; }

    }
}
