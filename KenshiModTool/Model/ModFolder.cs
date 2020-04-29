using Core;
using Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KenshiModTool.Model
{
    public class ModFolder
    {

        public ModFolder(Mod mod)
        {
            DisplayName = mod.DisplayName;
            FilePath = mod.FilePath;
            UniqueIdentifier = mod.UniqueIdentifier;
            Source = mod.Source;
            
            var symbLink = System.IO.Path.Combine(LoadService.config.GamePath, "Mods", System.IO.Path.GetFileNameWithoutExtension(mod.FilePath));
            HasSymbLink = Directory.Exists(symbLink) && LoadService.IsSymbolic(symbLink);

        }

        public string Color { get; set; }
        public Guid UniqueIdentifier { get; }
        public SourceEnum Source { get; }
        public string DisplayName { get; }
        public string FilePath { get; }
        public bool HasSymbLink { get; set; }
    }
}
