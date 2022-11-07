using HarmonyLib;
using RimWorld;
using System;
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

            __result = overseer.mechanitor.CanCommandTo(target);
            return;
        }
    }
}
