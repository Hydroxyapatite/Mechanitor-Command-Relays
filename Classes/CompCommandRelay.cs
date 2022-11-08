using Hydroxyapatite_MechanitorCommandRelays.Classes;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using static UnityEngine.GraphicsBuffer;

namespace Hydroxyapatite_MechanitorCommandRelays
{
    [StaticConstructorOnStartup]
    public class CompCommandRelay : ThingComp
    {
        private static readonly Material UntunedMaterial = SolidColorMaterials.SimpleSolidColorMaterial(Color.red);

        private static readonly Material TuningMaterial = SolidColorMaterials.SimpleSolidColorMaterial(Color.yellow);

        private static readonly Material TunedMaterial = SolidColorMaterials.SimpleSolidColorMaterial(Color.green);

        private static readonly Vector2 TuningBarSize = new Vector3(0.255f, 0.035f);

        private const float TuningBarYOffset = -0.4f;

        public Pawn tunedTo;

        public int tuningTimeLeft;

        public Pawn tuningTo;

        private Effecter effecter;

        private CompPowerTrader PowerTrader => parent.TryGetComp<CompPowerTrader>();

        public CompProperties_CommandRelay Props => (CompProperties_CommandRelay)props;

        private int RetuneTimeTicks => (int)(60000f * Props.retuneDays);

        private int TuningTimeTicks => (int)(Props.tuneSeconds * 60f);

        private CommandRelayState State
        {
            get
            {
                if (tunedTo != null && tuningTo != null)
                {
                    return CommandRelayState.Retuning;
                }
                if (tuningTo != null)
                {
                    return CommandRelayState.Tuning;
                }
                if (tunedTo != null)
                {
                    return CommandRelayState.Tuned;
                }
                return CommandRelayState.Untuned;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (!ModLister.CheckBiotech("Band node"))
            {
                parent.Destroy();
            }
            else
            {
                base.PostSpawnSetup(respawningAfterLoad);
            }
        }

        public override void PostExposeData()
        {
            Scribe_References.Look(ref tunedTo, "tunedTo");
            Scribe_References.Look(ref tuningTo, "tuningTo");
            Scribe_Values.Look(ref tuningTimeLeft, "tuningTimeLeft", 0);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            Command_Action command_Action = new Command_Action();
            command_Action.defaultLabel = ((tunedTo == null) ? ("CommandRelayTuneTo".Translate() + "...") : ("CommandRelayRetuneTo".Translate() + "..."));
            command_Action.defaultDesc = ((tunedTo == null) ? "CommandRelayTuningDesc".Translate("PeriodSeconds".Translate(Props.tuneSeconds)) : "CommandRelayRetuningDesc".Translate(Props.retuneDays + " " + "Days".Translate()));
            command_Action.onHover = (Action)Delegate.Combine(command_Action.onHover, (Action)delegate
            {
                Pawn pawn2 = ((tuningTo != null) ? tuningTo : tunedTo);
                if (pawn2 != null)
                {
                    GenDraw.DrawLineBetween(parent.DrawPos, pawn2.DrawPos);
                }
            });
            bool flag = false;
            foreach (Pawn item in parent.Map.mapPawns.AllPawnsSpawned)
            {
                if (MechanitorUtility.IsMechanitor(item) && tunedTo != item && tuningTo != item)
                {
                    flag = true;
                    break;
                }
            }
            command_Action.disabled = !flag;
            command_Action.icon = ContentFinder<Texture2D>.Get("UI/Gizmos/BandNodeTuning");
            command_Action.action = (Action)Delegate.Combine(command_Action.action, (Action)delegate
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (Pawn pawn in parent.Map.mapPawns.AllPawnsSpawned)
                {
                    if (MechanitorUtility.IsMechanitor(pawn))
                    {
                        // Get selected relays that are not tuned to or tuning to the mechanitor
                        IEnumerable<CompCommandRelay> commandRelays = from t in Find.Selector.SelectedObjects
                                                              where t is Thing thing && thing.TryGetComp<CompCommandRelay>() != null
                                                              select ((Thing)t).TryGetComp<CompCommandRelay>() into n
                                                              where n.tunedTo != pawn && n.tuningTo != pawn
                                                              select n;
                        if (commandRelays.Any())
                        {
                            Pawn localPawn = pawn;
                            string text = pawn.Name.ToStringFull;
                            // If all the relays are tuned or untuned:
                            if (commandRelays.All((CompCommandRelay b) => b.tunedTo == null) || commandRelays.All((CompCommandRelay b) => b.tunedTo != null))
                            {
                                // use retune text if the selected relay is tuned, or tune text otherwise.
                                text = ((tunedTo != null) ? (text + " (" + RetuneTimeTicks.ToStringTicksToPeriod() + ")") : ((string)(text + (" (" + Props.tuneSeconds + " " + "SecondsLower".Translate() + ")"))));
                            }
                            // Add the pawn to the list of options for tuning.
                            list.Add(new FloatMenuOption(text, delegate
                            {
                                foreach (CompCommandRelay item2 in commandRelays)
                                {
                                    item2.TuneTo(localPawn);
                                }
                            }));
                        }
                    }
                }
                Find.WindowStack.Add(new FloatMenu(list));
            });
            yield return command_Action;
            if (DebugSettings.ShowDevGizmos)
            {
                Command_Action command_Action2 = new Command_Action();
                command_Action2.defaultLabel = "DEV: complete tuning";
                command_Action2.action = delegate
                {
                    tuningTimeLeft = 0;
                };
                yield return command_Action2;
            }
        }

        public void TuneTo(Pawn pawn)
        {
            tuningTimeLeft = ((tunedTo == null) ? TuningTimeTicks : RetuneTimeTicks);
            tuningTo = pawn;
        }

        public override void PostDraw()
        {
            base.PostDraw();
            Material material = State switch
            {
                CommandRelayState.Tuned => TunedMaterial,
                CommandRelayState.Untuned => UntunedMaterial,
                _ => TuningMaterial
            };
            Vector3 s = new Vector3(TuningBarSize.x, 1f, TuningBarSize.y);
            Vector3 pos = parent.DrawPos + new Vector3(0f, 0f, TuningBarYOffset);
            pos.y = parent.def.altitudeLayer.AltitudeFor() + 3f / 74f;
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(pos, parent.Rotation.AsQuat, s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, material, 0);

            // If the mechanitor is spawned and any of their mechs are drafted:
            if ((tunedTo?.Spawned == true || tunedTo?.IsCaravanMember() == true) && tunedTo?.mechanitor?.AnySelectedDraftedMechs == true)
            {
                // draw the command range as a circle, adding cells that can be commanded to outside of the initially drawn radius.
                GenDraw.DrawRadiusRing(parent.Position, CompProperties_CommandRelay.relayCommandRange, Color.white, (IntVec3 c) => parent.Position.DistanceTo(c) < Math.Pow(CompProperties_CommandRelay.relayCommandRange, 2));
            }
        }

        public override void CompTick()
        {
            PowerTrader.PowerOutput = ((tunedTo == null && tuningTo == null) ? ((float)(-Props.powerConsumptionIdle)) : (0f - PowerTrader.Props.PowerConsumption));
            if (tunedTo != null && tunedTo.Dead)
            {
                tunedTo = null;
            }
            if (tuningTo != null && tuningTo.Dead)
            {
                tuningTo = null;
            }
            if (PowerTrader != null && !PowerTrader.PowerOn)
            {
                effecter?.Cleanup();
                effecter = null;
                return;
            }
            if (tuningTo != null)
            {
                tuningTimeLeft--;
                if (tuningTimeLeft <= 0)
                {
                    tunedTo = tuningTo;
                    tuningTo = null;
                    if (Props.tuningCompleteSound != null)
                    {
                        Props.tuningCompleteSound.PlayOneShot(parent);
                    }
                }
            }
            if (tuningTo == null && tunedTo != null && !tunedTo.health.hediffSet.HasHediff(Props.hediff))
            {
                tunedTo.health.AddHediff(Props.hediff, tunedTo.health.hediffSet.GetBrain());
            }
            if (State == CommandRelayState.Untuned)
            {
                if (effecter == null || effecter.def != Props.untunedEffect)
                {
                    effecter?.Cleanup();
                    effecter = Props.untunedEffect.Spawn();
                }
            }
            else if (State == CommandRelayState.Tuned)
            {
                if (effecter == null || effecter.def != Props.tunedEffect)
                {
                    effecter?.Cleanup();
                    effecter = Props.tunedEffect.Spawn();
                }
            }
            else if (State == CommandRelayState.Tuning)
            {
                if (effecter == null || effecter.def != Props.tuningEffect)
                {
                    effecter?.Cleanup();
                    effecter = Props.tuningEffect.Spawn();
                }
            }
            else if (State == CommandRelayState.Retuning)
            {
                if (effecter == null || effecter.def != Props.retuningEffect)
                {
                    effecter?.Cleanup();
                    effecter = Props.retuningEffect.Spawn();
                }
            }
            else
            {
                effecter?.Cleanup();
                effecter = null;
            }
            if (effecter != null)
            {
                effecter.EffectTick(parent, parent);
            }
        }

        public override string CompInspectStringExtra()
        {
            string text = null;
            if (!PowerTrader.PowerOn)
            {
                text = "\n" + "Unpowered".Translate().CapitalizeFirst().Resolve();
            }
            if (tuningTo != null)
            {
                return "CommandRelayTuningTo".Translate() + ": " + tuningTo.Name.ToStringFull + " - " + tuningTimeLeft.ToStringTicksToPeriod() + text;
            }
            return "CommandRelayTunedTo".Translate() + ": " + ((tunedTo == null) ? "Nobody".Translate().Resolve() : tunedTo.Name.ToStringFull) + text;
        }
    }
}
