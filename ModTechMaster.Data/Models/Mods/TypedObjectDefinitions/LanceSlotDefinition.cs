﻿namespace ModTechMaster.Data.Models.Mods.TypedObjectDefinitions
{
    using System;
    using System.Collections.Generic;

    using ModTechMaster.Core.Constants;
    using ModTechMaster.Core.Enums.Mods;
    using ModTechMaster.Core.Interfaces.Models;

    using Newtonsoft.Json.Linq;

    public class LanceSlotDefinition : ObjectDefinition
    {
        public LanceSlotDefinition(
            ObjectType objectType,
            IObjectDefinitionDescription objectDescription,
            dynamic jsonObject,
            string filePath,
            long lanceSlotNumber)
            : base(objectType, objectDescription, jsonObject as JObject, filePath)
        {
            this.LanceSlotNumber = lanceSlotNumber;
        }

        public List<string> ExcludedPilotTags { get; } = new List<string>();

        public List<string> ExcludedUnitTags { get; } = new List<string>();

        public long LanceSlotNumber { get; }

        public ObjectType LanceSlotObjectType { get; set; }

        public string PilotId { get; set; }

        public List<string> PilotTags { get; } = new List<string>();

        public string UnitId { get; set; }

        public List<string> UnitTags { get; } = new List<string>();

        public override void AddMetaData()
        {
            // Hacky, need to fix this shit up. If only this was my day job.
            this.MetaData.Add(Keywords.Id, $"{base.Id} - Slot {this.LanceSlotNumber}");
            this.MetaData.Add(Keywords.Name, $"{base.Id} - Slot {this.LanceSlotNumber}");

            base.AddMetaData();

            var lanceUnit = this.JsonObject;

            var unitType = lanceUnit.unitType.ToString();
            var unitId = lanceUnit.unitId.ToString();
            var pilotId = lanceUnit.pilotId.ToString();

            if (unitId != "Tagged")
            {
                switch (unitType)
                {
                    case "Mech":
                        this.LanceSlotObjectType = ObjectType.MechDef;
                        this.MetaData.Add(Keywords.MechDefId, this.UnitId);
                        break;
                    case "Turret":
                        this.LanceSlotObjectType = ObjectType.TurretDef;
                        this.MetaData.Add(Keywords.TurretId, this.UnitId);
                        break;
                    case "Vehicle":
                        this.LanceSlotObjectType = ObjectType.VehicleDef;
                        this.MetaData.Add(Keywords.VehicleId, this.UnitId);
                        break;
                    case "UNDEFINED":
                        throw new InvalidProgramException();
                }
            }

            this.UnitId = unitId;
            this.PilotId = pilotId;
            if (this.PilotId != "Tagged")
            {
                this.MetaData.Add(Keywords.PilotId, this.UnitId);
            }

            if (lanceUnit.unitTagSet?.items != null)
            {
                foreach (var tagItem in lanceUnit.unitTagSet.items)
                {
                    this.UnitTags.Add(tagItem.ToString());
                }
            }

            if (lanceUnit.excludedUnitTagSet?.items != null)
            {
                foreach (var tagItem in lanceUnit.excludedUnitTagSet.items)
                {
                    this.ExcludedUnitTags.Add(tagItem.ToString());
                }
            }

            if (lanceUnit.pilotTagSet?.items != null)
            {
                foreach (var tagItem in lanceUnit.pilotTagSet.items)
                {
                    this.PilotTags.Add(tagItem.ToString());
                }
            }

            if (lanceUnit.excludedPilotTagSet?.items != null)
            {
                foreach (var tagItem in lanceUnit.excludedPilotTagSet.items)
                {
                    this.ExcludedPilotTags.Add(tagItem.ToString());
                }
            }

            this.Tags.Add(Keywords.UnitTags, this.UnitTags);
            this.Tags.Add(Keywords.ExcludedUnitTags, this.ExcludedUnitTags);
            this.Tags.Add(Keywords.PilotTags, this.PilotTags);
            this.Tags.Add(Keywords.ExcludedPilotTags, this.ExcludedPilotTags);
        }
    }
}