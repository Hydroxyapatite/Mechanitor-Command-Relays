using RimWorld;
using UnityEngine;
using Verse;

namespace Hydroxyapatite_MechanitorCommandRelays
{
    public class PlaceWorker_CommandRelay : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            CompProperties_CommandRelay compProperties = def.GetCompProperties<CompProperties_CommandRelay>();
            if (compProperties != null)
            {
                GenDraw.DrawRadiusRing(center, CompProperties_CommandRelay.relayCommandRange, Color.white);
            }
        }
    }
}
