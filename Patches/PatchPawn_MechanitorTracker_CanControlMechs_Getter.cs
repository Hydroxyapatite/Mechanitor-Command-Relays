using RimWorld;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace Hydroxyapatite_MechanitorCommandRelays
{
    [HarmonyPatch(
            typeof(Pawn_MechanitorTracker),
            nameof(Pawn_MechanitorTracker.CanControlMechs),
            MethodType.Getter)]
    public class PatchPawn_MechanitorTracker_CanControlMechs_Getter
    {
        static void Postfix(
            Pawn_MechanitorTracker __instance,
            ref AcceptanceReport __result)
        {
            // Only run extra check if we hit the condition to get "false" (not spawned and not in a building)
            if(__result == false)
            {
                if (__instance.Pawn.IsCaravanMember())
                {
                    __result = true;
                    return;
                }
            }
        }
    }
}
