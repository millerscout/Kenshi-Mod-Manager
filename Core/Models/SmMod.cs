using System;
using System.Collections.Generic;

namespace Core.Models
{
    public class SmMod
    {
        public Guid UniqueIdentifier { get; set; }
        public string FileName { get; set; }
        public List<string> References { get; set; }
        public List<string> Dependencies { get; set; }
        public bool OrderedAutomatically { get; set; } = false;
        public int Order { get; set; }
    }
}