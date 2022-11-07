using Hydroxyapatite_MechanitorCommandRelays.Classes;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Hydroxyapatite_MechanitorCommandRelays
{
    internal class Hediff_CommandRelay : Hediff
    {
        private const int BandNodeCheckInterval = 60;

        private int cachedTunedCommandRelaysCount;

        private HediffStage curStage;

        public override bool ShouldRemove => cachedTunedCommandRelaysCount == 0;

        public List<Building> cachedTunedCommandRelays = new List<Building>();

        public override HediffStage CurStage
        {
            get
            {
                if (curStage == null && cachedTunedCommandRelaysCount > 0)
                {
                    curStage = new HediffStage();
                }
                return curStage;
            }
        }

        public override void PostTick()
        {
            base.PostTick();
            if (pawn.IsHashIntervalTick(60))
            {
                RecacheCommandRelays();
            }
        }

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            RecacheCommandRelays();
        }

        public void RecacheCommandRelays()
        {
            int num = cachedTunedCommandRelaysCount;
            cachedTunedCommandRelaysCount = 0;
            List<Map> maps = Find.Maps;
            List<Building> tunedRelays = new List<Building>();

            for (int i = 0; i < maps.Count; i++)
            {
                foreach (Building item in maps[i].listerBuildings.AllBuildingsColonistOfDef(CommandRelayDefOf.Hydroxyapatite_CommandRelay))
                {
                    if (item.TryGetComp<CompCommandRelay>().tunedTo == pawn && item.TryGetComp<CompPowerTrader>().PowerOn)
                    {
                        cachedTunedCommandRelaysCount++;
                        tunedRelays.Add(item);
                    }
                }
            } 

            cachedTunedCommandRelays = tunedRelays;
            MechanitorRelayCacheHelper.relayCache[pawn.mechanitor] = tunedRelays;
            if (num != cachedTunedCommandRelaysCount)
            {
                curStage = null;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref cachedTunedCommandRelaysCount, "cachedTunedcommandRelaysCount", 0);
        }
    }
}
