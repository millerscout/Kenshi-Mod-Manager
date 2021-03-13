using System;
using System.Collections.Generic;
using System.Text;

namespace Core.LocalData.Models
{
    public ref struct Mod
    {
        public Mod(int id, string name, string hash, string version)
        {
            Id = id;
            Name = name;
            Hash = hash;
            Version = version;
        }

        public int Id { get; }
        public string Name { get; }
        public string Hash { get; }
        public string Version { get; }

    }
}
