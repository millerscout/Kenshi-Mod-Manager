using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Kenshi_Data.Model
{
    public class Header
    {
        public string Author = "";
        public string Description = "";
        public int Version = 1;
        public List<string> Dependencies;
        public List<string> Referenced;
    }
}
