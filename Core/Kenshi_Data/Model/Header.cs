using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Kenshi_Data.Model
{
    public struct Header
    {
        public string Author { get; }
        public string Description { get; }
        public int Version { get; }
        public string[] Dependencies { get; }
        public string[] Referenced { get; }

        public Header(string author, string description, int version, string[] dependencies, string[] referenced)
        {
            Author = author;
            Description = description;
            Version = version;
            Dependencies = dependencies;
            Referenced = referenced;
        }
    }
}
