using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace InjuryNotifier
{
    public class MapComponent_InjuryNotifier : MapComponent
    {
        public int slowDown = 0;
        public HashSet<PawnPartProblem> set = new HashSet<PawnPartProblem>();
        public bool firstRun = true;

        public MapComponent_InjuryNotifier(Map map) : base(map)
        {
            LongEventHandler.QueueLongEvent(ensureComponentExists, null, false, null);
        }

        public static void ensureComponentExists()
        {
            foreach (Map m in Find.Maps)
            {
                if (m.GetComponent<MapComponent_InjuryNotifier>() == null)
                {
                    m.components.Add(new MapComponent_InjuryNotifier(m));
                }
            }
        }

        public override void MapComponentTick()
        {
            slowDown++;
            if (slowDown > 100)
            {
                slowDown = 0;
                if (firstRun)
                {
                    updateCollection(false);
                    firstRun = false;
                }
                else
                {
                    updateCollection(true);
                }
            }
        }

        public void updateCollection(bool notify)
        {
            //Log.Message("Hello from InjuryNotifier updateCollection BEGIN");
            foreach (Pawn p in map.mapPawns.FreeColonistsAndPrisonersSpawned)
            {
                foreach (Hediff_MissingPart hmp in p.health.hediffSet.GetMissingPartsCommonAncestors())
                {
                    //Log.Message(p.NameStringShort + " lost " + hmp.Part.def.label + " due to " + hmp.Label + "!");
                    bool isNew = false;
                    isNew = set.Add(new PawnPartProblem(p.thingIDNumber, hmp.Part.def.label, trimInjuryLabel(hmp.Label)));
                    if (notify && isNew)
                    {
                        //Log.Message(p.NameStringShort + " lost " + hmp.Part.def.label + " due to " + hmp.Label + "!");
                        Find.LetterStack.ReceiveLetter("Lost Part", p.Name.ToStringShort + " lost their " + hmp.Part.def.label + " due to " + trimInjuryLabel(hmp.Label) + "!", RimWorld.LetterDefOf.NegativeEvent, new GlobalTargetInfo(p));
                    }
                }
                foreach (Hediff h in p.health.hediffSet.hediffs)
                {
                    if (h is Hediff_Injury)
                    {
                        Hediff_Injury hi = (Hediff_Injury)h;
                        if (hi.IsPermanent())
                        {
                            //Log.Message(p.NameStringShort + " has new " + hi.Label + " on their " + hi.Part.def.label + "!");
                            bool isNew = false;
                            isNew = set.Add(new PawnPartProblem(p.thingIDNumber, hi.Part.def.label, trimInjuryLabel(hi.Label)));
                            if (notify && isNew)
                            {
                                //Log.Message(p.NameStringShort + " has new " + hi.Label + " on their " + hi.Part.def.label + "!");
                                Find.LetterStack.ReceiveLetter("New Scar", p.Name.ToStringShort + " has new " + trimInjuryLabel(hi.Label) + " on their " + hi.Part.def.label + "!", RimWorld.LetterDefOf.NegativeEvent, new GlobalTargetInfo(p));
                            }
                        }
                    }
                }
            }
            //Log.Message("Hello from InjuryNotifier updateCollection END");
        }

        public static string trimInjuryLabel(string s)
        {
            int leftParenIndex = s.IndexOf(" (");
            if (leftParenIndex > 0)
            {
                return s.Substring(0, leftParenIndex);
            }
            else
            {
                return s;
            }
        }

    }

    public struct PawnPartProblem
    {
        public readonly int pawnIDNumber;
        public readonly string part_label;
        public readonly string hediff_label;

        public PawnPartProblem(int pawnIDNumber, string part_label, string hediff_label)
        {

            this.pawnIDNumber = pawnIDNumber;
            this.part_label = part_label;
            this.hediff_label = hediff_label;

        }

        public override int GetHashCode()
        {
            return pawnIDNumber.GetHashCode() * part_label.GetHashCode() * hediff_label.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is PawnPartProblem && Equals((PawnPartProblem) obj);
        }

        public bool Equals(PawnPartProblem ppp)
        {
            return ppp.pawnIDNumber == this.pawnIDNumber && ppp.part_label == this.part_label && ppp.hediff_label == this.hediff_label;
        }
    }

}
