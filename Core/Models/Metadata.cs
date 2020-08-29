using System.Collections.Generic;

namespace Core.Models
{
    public class Metadata
    {
        public Metadata()
        {
            Dependencies = new List<string>();
            Referenced = new List<string> { };
        }

        public string Author = "";
        public string Description = "";
        public int Version = 1;
        public List<string> Dependencies;
        public List<string> Referenced;
    }
}