using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Core.Models
{
    public class Mod
    {
        public Guid UniqueIdentifier = Guid.NewGuid();
        public SourceEnum Source { get; set; }
        public string Id { get; set; }
        public int Order { get; set; }
        public IEnumerable<string> Categories { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }

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
                return Path.GetFileNameWithoutExtension(Name);
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

    }
}
