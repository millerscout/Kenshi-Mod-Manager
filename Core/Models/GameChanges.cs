using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{

    public class GameChanges
    {
        public string State { get; set; }
        public string ModName { get; set; }
        public object Value { get; set; }
    }
}
