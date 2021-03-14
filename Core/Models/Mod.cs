using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Core.Models
{
    public class Mod
    {
        public Mod(string filePath)
        {
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            DisplayName = Path.HasExtension(FilePath) ? Path.GetFileNameWithoutExtension(FilePath) : $"[Error]: {Path.GetFileNameWithoutExtension(FilePath)}";
        }
        public Guid UniqueIdentifier = Guid.NewGuid();
        public SourceEnum Source { get; set; }
        public string Id { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public int Order { get; set; }
        public IEnumerable<string> Categories { get; set; }
        public IEnumerable<string> TypesChanged { get; set; }
        public string FilePath { get; set; }
        public bool Active { get; set; }
        public string Color { get; set; }

        public Mod setActive(bool newValue)
        {
            this.Active = newValue;
            return this;
        }

        public string Url
        {
            get
            {
                if (Source == SourceEnum.Steam)
                {
                    return LoadService.config.SteamPageUrl + Id;
                }
                else return LoadService.config.NexusPageUrl + DisplayName;
            }
        }

        public string DisplayName { get; }

        public string FileName { get; }


        public List<string> Dependencies = new List<string>();

        /// <summary>
        /// don`t know what is this used for... but seems something important while ordering.
        /// </summary>
        public List<string> References = new List<string>();

        public IEnumerable<string> AllDependencies => Dependencies.Concat(References).Where(c => !Constants.BaseMods.Contains(c.ToLower()));
        public bool OrderedAutomatically { get; set; }
    }

    public class Conflict
    {
        public string ItemChangeName { get; set; }

        public List<ConflictItem> Items { get; set; }
        public string Name { get; set; }
        public string Property { get; set; }
    }

    public class ConflictItem
    {
        public string Mod { get; set; }
        public object ItemValue { get; set; }
        public bool Priority { get; set; }
        public string State { get; set; }
        public int Order { get; set; }
    }
}