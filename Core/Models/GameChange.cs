using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{

    public class GameChange
    {
        public string State { get; set; }
        public string ModName { get; set; }
        public object Value { get; set; }
    }
}
