﻿namespace ModTechMaster.Logic.Processors
{
    using System.Diagnostics;
    using System.IO;

    using ModTechMaster.Core.Enums.Mods;
    using ModTechMaster.Core.Interfaces.Models;
    using ModTechMaster.Core.Interfaces.Processors;
    using ModTechMaster.Core.Interfaces.Services;
    using ModTechMaster.Data.Models.Mods;
    using ModTechMaster.Logic.Factories;

    using Newtonsoft.Json;

    internal class ObjectDefinitionProcessor : IObjectDefinitionProcessor
    {
        public IObjectDefinition ProcessObjectDefinition(
            IManifestEntry manifestEntry,
            DirectoryInfo di,
            FileInfo fi,
            IReferenceFinderService referenceFinderService,
            ObjectType? objectTypeOverride = null)
        {
            if (fi.Extension != ".json")
            {
                Debug.WriteLine($"File {fi.FullName} is not a JSON file.");
                return null;
            }

            dynamic json = JsonConvert.DeserializeObject(File.ReadAllText(fi.FullName));
            return ObjectDefinitionFactory.ObjectDefinitionFactorySingleton.Get(
                objectTypeOverride ?? manifestEntry.EntryType,
                ProcessObjectDescription(json.Description),
                json,
                fi.FullName,
                referenceFinderService);
        }

        private static IObjectDefinitionDescription ProcessObjectDescription(dynamic description)
        {
            if (description == null)
            {
                return null;
            }

            string id = description.Id != null ? description.Id.ToString() : null;
            string name = description.Name != null ? description.Name.ToString() : null;
            string desc = description.Description != null ? description.Description.ToString() : null;
            string icon = description.Icon != null ? description.Icon.ToString() : null;
            return new ObjectDefinitionDescription(id, name, desc, icon, description);
        }
    }
}