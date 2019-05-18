﻿using ModTechMaster.Core.Constants;
using ModTechMaster.Core.Enums.Mods;
using ModTechMaster.Core.Interfaces.Models;
using Newtonsoft.Json.Linq;

namespace ModTechMaster.Data.Models.Mods.TypedObjectDefinitions
{
    public class CastObjectDefinition : ObjectDefinition
    {
        public CastObjectDefinition(ObjectType objectType, IObjectDefinitionDescription objectDescription,
            dynamic jsonObject, string filePath) : base(objectType, objectDescription, (JObject) jsonObject, filePath)
        {
        }

        public override void AddMetaData()
        {
            base.AddMetaData();
            MetaData.Add(Keywords.Faction, JsonObject.faction);
            this.MetaData.Add(Keywords.PortraitPath, this.JsonObject.defaultEmotePortrait.portraitAssetPath);
        }
    }
}