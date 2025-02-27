﻿namespace ModTechMaster.Logic.Processors
{
    using System;
    using System.IO;
    using System.Linq;

    using ModTechMaster.Core.Enums.Mods;
    using ModTechMaster.Core.Interfaces.Models;
    using ModTechMaster.Core.Interfaces.Processors;
    using ModTechMaster.Core.Interfaces.Services;
    using ModTechMaster.Data.Models.Mods;
    using ModTechMaster.Logic.Factories;

    using Newtonsoft.Json;

    public class CustomComponentsManifestEntryProcessor : IManifestEntryProcessor
    {
        public IManifestEntry ProcessManifestEntry(
            IManifest manifest,
            ObjectType entryType,
            string path,
            dynamic jsonObject,
            IReferenceFinderService referenceFinderService)
        {
            var manifestEntry = new ManifestEntry(manifest, entryType, path, jsonObject, referenceFinderService);

            var di = new DirectoryInfo(Path.Combine(manifest.Mod.SourceDirectoryPath, manifestEntry.Path));
            var files = di.EnumerateFiles();

            if (files.Count() > 1)
            {
                throw new InvalidProgramException(
                    $"Encountered more than ONE CC settings files for a CC Manifest Entry at [{di.FullName}]");
            }

            var file = files.First();

            dynamic ccSettingsData = JsonConvert.DeserializeObject(File.ReadAllText(file.FullName));
            foreach (var ccSetting in ccSettingsData.Settings)
            {
                var objectDefinition =
                    ObjectDefinitionFactory.ObjectDefinitionFactorySingleton.Get(
                        entryType,
                        null,
                        ccSetting,
                        file.FullName,
                        referenceFinderService);
                manifestEntry.Objects.Add(objectDefinition);
            }

            return manifestEntry;
        }
    }
}