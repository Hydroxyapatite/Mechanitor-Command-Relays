using HarmonyLib;
using Verse;


namespace Hydroxyapatite_MechanitorCommandRelays
{
    [StaticConstructorOnStartup]
    public class Main
    {
        static Main()
        {
            var harmony = new Harmony("rimworld.hydroxyapatite.MechCommandRelays");
            harmony.PatchAll();
        }
    }
}
