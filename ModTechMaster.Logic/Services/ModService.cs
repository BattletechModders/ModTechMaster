﻿namespace ModTechMaster.Logic.Services
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;

    using Castle.Core.Logging;

    using Framework.Utils.Directory;

    using ModTechMaster.Core.Enums;
    using ModTechMaster.Core.Enums.Mods;
    using ModTechMaster.Core.Interfaces.Factories;
    using ModTechMaster.Core.Interfaces.Models;
    using ModTechMaster.Core.Interfaces.Services;
    using ModTechMaster.Data.Annotations;
    using ModTechMaster.Data.Models.Mods;
    using ModTechMaster.Data.Models.Mods.TypedObjectDefinitions;
    using ModTechMaster.Logic.Factories;
    using ModTechMaster.Logic.Processors;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class ModService : IModService
    {
        private readonly ILogger logger;

        private readonly IManifestEntryProcessorFactory manifestEntryProcessorFactory;

        private readonly IMessageService messageService;

        private readonly IReferenceFinderService referenceFinderService;

        public ModService(
            IMessageService messageService,
            IManifestEntryProcessorFactory manifestEntryProcessorFactory,
            ILogger logger,
            IReferenceFinderService referenceFinderService)
        {
            this.messageService = messageService;
            this.manifestEntryProcessorFactory = manifestEntryProcessorFactory;
            this.logger = logger;
            this.referenceFinderService = referenceFinderService;
            this.ModCollection = new ModCollection("Unknown Mod Collection", string.Empty);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public IModCollection ModCollection { get; }

        public IModCollection LoadCollectionFromPath(string battleTechPath, string modsPath, string name)
        {
            if (!DirectoryUtils.Exists(modsPath))
            {
                throw new Exception($@"The specified Mods directory [{modsPath}] does not exist.");
            }

            if (!DirectoryUtils.Exists(battleTechPath))
            {
                throw new Exception($@"The specified Battle Tech directory [{battleTechPath}] does not exist.");
            }

            /*var modsDirectoryInfo = new DirectoryInfo(modsPath);
            this.ModCollection.Name = name;
            this.ModCollection.Path = modsPath;

            this.logger.Info($"Processing mods from [{modsDirectoryInfo.FullName}]");

            modsDirectoryInfo.GetDirectories().AsParallel().ForAll(
                sub =>
                    {
                        this.logger.Debug(".");
                        var mod = this.TryLoadFromPath(sub.FullName, false);
                        this.ModCollection.AddModToCollection(mod);
                    });*/

            var gameDirectoryInfo = new DirectoryInfo(battleTechPath);
            this.logger.Info($"Processing BattleTech from [{gameDirectoryInfo.FullName}]");
            var battleTechMod = this.TryLoadFromPath(gameDirectoryInfo.FullName, true);

            this.ModCollection.Mods.Sort(
                (mod, mod1) => string.Compare(mod.Name, mod1.Name, StringComparison.OrdinalIgnoreCase));

            this.OnPropertyChanged(nameof(this.ModCollection));

            return this.ModCollection;
        }

        public IMod TryLoadFromPath(string path, bool isBattleTechData)
        {
            if (!DirectoryUtils.Exists(path) || (!isBattleTechData && !File.Exists(ModFilePath(path))))
            {
                return null;
            }

            return this.LoadFromPath(path, isBattleTechData);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static string ModFilePath(string path)
        {
            return Path.Combine(path, @"mod.json");
        }

        private void AddStreamingAssetsManifestEntry(string simPath, Mod mod, Manifest manifest)
        {
            var newEntry = new ManifestEntry(
                manifest,
                ObjectType.StreamingAssetsData,
                simPath,
                null,
                this.referenceFinderService);
            //newEntry.ParseStreamingAssets();
            this.RecurseStreamingAssetsDirectory(simPath, newEntry);
            manifest.Entries.Add(newEntry);
        }

        private void RecurseStreamingAssetsDirectory(string path, ManifestEntry newEntry)
        {
            var di = new DirectoryInfo(path);
            foreach (var fi in di.EnumerateFiles())
            {
                var fileName = fi.Name;
                var fileData = File.ReadAllText(fi.FullName);
                var hostDirectory = di.Name;

                var retVal = StreamingAssetProcessor.ProcessFile(
                    newEntry,
                    di,
                    fi.FullName,
                    fileData,
                    hostDirectory,
                    this.referenceFinderService);

                if (retVal != null)
                {
                    if (retVal is IObjectDefinition obj)
                    {
                        newEntry.Objects.Add(obj);
                    }
                    else if (retVal is IResourceDefinition res)
                    {
                        newEntry.Resources.Add(res);
                    }
                    else
                    {
                        throw new InvalidProgramException($"Unknown streaming asset object.");
                    }
                }
            }

            di
                .GetDirectories()
                //.ToList().ForEach(
                .AsParallel().ForAll(
                    subdi => this.RecurseStreamingAssetsDirectory(subdi.FullName, newEntry));
        }

        private Mod InitModFromJson(dynamic src, string path)
        {
            var fi = new FileInfo(path);
            var di = new DirectoryInfo(fi.DirectoryName);
            var depends = src.DependsOn == null
                              ? new HashSet<string>()
                              : new HashSet<string>(((JArray)src.DependsOn).Select(token => token.ToString()));
            var conflicts = src.ConflictsWith == null
                                ? new HashSet<string>()
                                : new HashSet<string>(((JArray)src.ConflictsWith).Select(token => token.ToString()));
            var name = src.Name.ToString();
            var enabled = (bool?)src.Enabled;
            var version = src.Version?.ToString();
            var description = src.Description?.ToString();
            var author = src.Author?.ToString();
            var website = src.Website?.ToString();
            var contact = src.Contact?.ToString();
            var dll = src.DLL?.ToString();
            return new Mod(
                name,
                enabled,
                version,
                description,
                author,
                website,
                contact,
                depends,
                conflicts,
                path,
                src,
                Convert.ToDouble(di.EnumerateFiles("*", SearchOption.AllDirectories).Sum(info => info.Length)) / 1024,
                dll);
        }

        private IMod LoadFromPath(string path, bool isBattleTechData)
        {
            Mod mod;
            if (!isBattleTechData)
            {
                dynamic src = JsonConvert.DeserializeObject(File.ReadAllText(ModFilePath(path)));
                mod = this.InitModFromJson(src, ModFilePath(path));
            }
            else
            {
                mod = new Mod("Battle Tech", true, "N/A", "Base Game Data", "HBS", "N/A", string.Empty, new HashSet<string>(), new HashSet<string>(), path, null, -1, null)
                          {
                              IsBattleTech = true
                          };
            }

            this.ProcessModConfig(mod);

            return mod;
        }

        private Manifest ProcessManifest(Mod mod)
        {
            var manifest = new Manifest(mod, mod.JsonObject.Manifest);
            if (manifest.JsonObject != null)
            {
                foreach (var manifestEntrySrc in manifest.JsonObject)
                {
                    ManifestEntry manifestEntry = this.ProcessManifestEntry(manifest, manifestEntrySrc);
                    if (manifestEntry == null)
                    {
                        throw new InvalidProgramException();
                    }

                    manifest.Entries.Add(manifestEntry);
                }
            }

            return manifest;
        }

        private ManifestEntry ProcessManifestEntry(Manifest manifest, dynamic manifestEntrySrc)
        {
            ObjectType entryType;
            if (!Enum.TryParse((string)manifestEntrySrc.Type, out entryType))
            {
                this.messageService.PushMessage(
                    $"Failed to parse Manifest Entry Type [{manifestEntrySrc.Type.ToString()}].",
                    MessageType.Error);
                return null;
            }

            var manifestEntryProcessor = this.manifestEntryProcessorFactory.Get(entryType);
            return manifestEntryProcessor.ProcessManifestEntry(
                manifest,
                entryType,
                (string)manifestEntrySrc.Path,
                manifestEntrySrc,
                this.referenceFinderService);
        }

        private void ProcessModConfig(Mod mod)
        {
            Manifest manifest;
            if (!mod.IsBattleTech)
            {
                // Process Manifest
                manifest = this.ProcessManifest(mod);
            }
            else
            {
                manifest = new Manifest(mod, null);
            }

            // Process implicits like StreamingAssets folder...
            // Special handling for sim game constants...
            var streamingAssetsPath = mod.IsBattleTech ? @"BattleTech_Data\\StreamingAssets\\data" : @"StreamingAssets";
            var fullPath = Path.Combine(mod.SourceDirectoryPath, mod.IsBattleTech ? "BattleTech" : string.Empty, streamingAssetsPath);
            if (Directory.Exists(fullPath))
            {
                this.AddStreamingAssetsManifestEntry(fullPath, mod, manifest);
            }

            if (manifest.Entries.Any())
            {
                mod.Manifest = manifest;
            }

            if (!mod.IsBattleTech)
            {
                var di = new DirectoryInfo(mod.SourceDirectoryPath);
                foreach (var file in di.EnumerateFiles())
                {
                    switch (file.Extension.ToLower())
                    {
                        case ".dll":
                            mod.ResourceFiles.Add(
                                new ResourceDefinition(ObjectType.Dll, file.FullName, file.Name, file.Name));
                            break;
                        default:
                            mod.ResourceFiles.Add(
                                new ResourceDefinition(
                                    ObjectType.UnhandledResource,
                                    file.FullName,
                                    file.Name,
                                    file.Name));
                            break;
                    }
                }
            }
        }
    }
}