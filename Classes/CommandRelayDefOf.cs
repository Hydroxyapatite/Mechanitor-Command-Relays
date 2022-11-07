using RimWorld;
using Verse;

namespace Hydroxyapatite_MechanitorCommandRelays
{
    [DefOf]
    public static class CommandRelayDefOf
    {
        [MayRequireBiotech]
        public static ThingDef Hydroxyapatite_CommandRelay;
        static CommandRelayDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(CommandRelayDefOf));
        }
    }
}
