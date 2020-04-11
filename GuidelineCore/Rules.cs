using Core.Models;
using System;
using System.Collections.Generic;

namespace GuidelineCore
{
    public class Rules
    {
        public int Order { get; set; }
        public int MaxRange { get; set; } = 5000;
        public string Name { get; set; }
        public List<string> Mod { get; set; }
        public List<Mod> ModsOrdered { get; set; } = new List<Mod>();


        public int InitialRange
        {
            get
            {
                return MaxRange * Order;
            }
        }
    }
}
