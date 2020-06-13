using Core.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Core.Models
{
    public class Rules
    {
        public int Order { get; set; }
        [JsonIgnore]
        public int MaxRange { get; set; } = 5000;
        public string Name { get; set; }
        public List<string> Mod { get; set; }
        [JsonIgnore]
        public List<Mod> ModsOrdered { get; set; } = new List<Mod>();

        [JsonIgnore]
        public int InitialRange
        {
            get
            {
                return MaxRange * Order;
            }
        }
    }
}