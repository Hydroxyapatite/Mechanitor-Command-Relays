using HarmonyLib;
using Hydroxyapatite_MechanitorCommandRelays.Classes;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace Hydroxyapatite_MechanitorCommandRelays
{
    [HarmonyPatch(
            typeof(MechanitorUtility),
            nameof(MechanitorUtility.InMechanitorCommandRange),
            new Type[] { typeof(Pawn), typeof(LocalTargetInfo) })]
    public class PatchMechanitorUtility_InMechanitorCommandRange
    {
        static void Postfix(
            Pawn mech,
            LocalTargetInfo target,
            ref bool __result
            )
        {
            if (__result == true)
            {
                return;
            }

            Pawn overseer = mech.GetOverseer();
            if (overseer.IsCaravanMember())
            {
                if (MechanitorRelayCacheHelper.relayCache.TryGetValue(overseer.mechanitor, out List<Building> relays))
                {
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
            else
            {
                __result = overseer.mechanitor.CanCommandTo(target);
            }
            
            return;
        }
    }
}
