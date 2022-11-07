using Verse;

namespace Hydroxyapatite_MechanitorCommandRelays
{
    public class CompProperties_CommandRelay : CompProperties
    {
        public HediffDef hediff;

        public float retuneDays = 3f;

        public float tuneSeconds = 5f;

        public int powerConsumptionIdle = 100;

        public int emissionInterval;

        public EffecterDef untunedEffect;

        public EffecterDef tuningEffect;

        public EffecterDef tunedEffect;

        public EffecterDef retuningEffect;

        public SoundDef tuningCompleteSound;

        public const float relayCommandRange = 14.9f;

        public CompProperties_CommandRelay()
        {
            compClass = typeof(CompCommandRelay);
        }
    }
}