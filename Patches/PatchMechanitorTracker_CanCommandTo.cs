using HarmonyLib;
using Hydroxyapatite_MechanitorCommandRelays.Classes;
using System.Collections.Generic;
using RimWorld;
using System;
using Verse;

namespace Hydroxyapatite_MechanitorCommandRelays
{
    [HarmonyPatch(
            typeof(Pawn_MechanitorTracker),
            nameof(Pawn_MechanitorTracker.CanCommandTo),
            new Type[] { typeof(LocalTargetInfo) })]
    public class PatchMechanitorTracker_CanCommandTo
    {
        static void Postfix(
            Pawn_MechanitorTracker __instance,
            LocalTargetInfo target,
            ref bool __result
            )
        {
            // Don't check relays if mechanitor is in range
            if(__result == true)
            {
                return;
            }

            // If there are relays tuned to the mechanitor, check whether they are in range            
            if(MechanitorRelayCacheHelper.relayCache.TryGetValue(__instance, out List<Building> relays)){
                foreach (Building relay in relays)
                {
                    if (target.Cell.InBounds(relay.MapHeld) && relay.Position.DistanceToSquared(target.Cell) < Math.Pow(CompProperties_CommandRelay.relayCommandRange, 2))
                    {
                        __result = true;
                        return;
                    }
                }
            }
        }
    }
}
