using Core;
using Core.Models;
using GuidelineCore;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace TestProject
{
    class Program
    {
        static void Main(string[] args)
        {

            ///Booting Mods.
            ///
            LoadService.Setup();
            var mods = LoadService.GetListOfMods();

            var rules = RuleService.GetRules();

            var orderedList = RuleService.OrderMods(mods);

        }


    }
}

