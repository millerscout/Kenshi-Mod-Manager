using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Core.Models
{
    public class Mod
    {
        public Guid UniqueIdentifier = Guid.NewGuid();
        public SourceEnum Source { get; set; }
        public string Id { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public int Order { get; set; }
        public IEnumerable<string> Categories { get; set; }
        public string FilePath { get; set; }
        public bool Active { get; set; }
        public bool OrderedAutomatically { get; set; }
        public string Color { get; set; }

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

        public string DisplayName
        {
            get
            {
                return Path.GetFileNameWithoutExtension(FilePath);
            }
        }

        public string FileName
        {
            get
            {
                return Path.GetFileName(FilePath);
            }
        }

        public string DisplayCategories
        {
            get
            {
                if (Categories == null || Categories.Count() == 0)
                    return "";
                return string.Join(",", Categories);
            }
        }

        public List<string> Dependencies { get; set; }

        /// <summary>
        /// don`t know what is this used for... but seems something important while ordering.
        /// </summary>
        public List<string> References { get; set; }
    }
}