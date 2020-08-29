using System.Collections.Concurrent;

namespace Core.Models
{
    public class ModListChanges
    {
        public ConcurrentStack<string> Mod { get; set; }
        public ConcurrentStack<GameChange> ChangeList { get; set; }
    }
}