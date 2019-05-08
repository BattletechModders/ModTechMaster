﻿using ModTechMaster.Data.Models.Mods;

namespace MTM
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using ModTechMaster.Core.Interfaces.Models;
    using ModTechMaster.Core.Interfaces.Services;
    using ModTechMaster.Logic.Factories;
    using ModTechMaster.Logic.Services;

    internal class Program
    {
        private static int Main(string[] args)
        {
            var di = new DirectoryInfo(args[0]);
            if (!di.Exists)
            {
                Console.WriteLine($"The target directory [{di.FullName}] foes not exist.");
                return -1;
            }

            IModService modService = new ModService(new MessageService(), new ManifestEntryProcessorFactory());
            IModCollection modCollection = new ModCollection("MTM Mod Collection");

            Console.WriteLine($"Processing mods from [{di.FullName}]");
            di.GetDirectories().ToList().ForEach(
                                                 sub =>
                                                 {
                                                     Console.Write(".");
                                                     modCollection.AddModToCollection(modService.TryLoadFromPath(sub.FullName));
                                                 });

            return 0;
        }
    }
}