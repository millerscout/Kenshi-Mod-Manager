using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class ModListChanges
    {

        public ConcurrentStack<string> Mod { get; set; }
        public ConcurrentStack<GameChange> ChangeList { get; set; }
    }
  
}
