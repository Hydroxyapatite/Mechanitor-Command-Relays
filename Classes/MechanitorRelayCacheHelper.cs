using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Hydroxyapatite_MechanitorCommandRelays.Classes
{
    [StaticConstructorOnStartup]
    public static class MechanitorRelayCacheHelper
    {
        public static Dictionary<Pawn_MechanitorTracker, List<Building>> relayCache = new Dictionary<Pawn_MechanitorTracker, List<Building>>();
    }
}
