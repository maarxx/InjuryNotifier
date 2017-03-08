using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace InjuryNotifier
{
    public class InjuryNotifier : MapComponent
    {
        public int slowDown = 0;
        public HashSet<PawnPartProblem> set = new HashSet<PawnPartProblem>();
        public bool firstRun = true;

        public InjuryNotifier(Map map) : base(map)
        {
            updateCollection(false); // Doesn't seem to work, no biggie.
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
                    isNew = set.Add(new PawnPartProblem(p.thingIDNumber, hmp.Part.def.label, hmp.Label));
                    if (notify && isNew)
                    {
                        //Log.Message(p.NameStringShort + " lost " + hmp.Part.def.label + " due to " + hmp.Label + "!");
                        Find.LetterStack.ReceiveLetter("Lost Part", p.NameStringShort + " lost their " + hmp.Part.def.label + " due to " + hmp.Label + "!", LetterType.BadUrgent, new GlobalTargetInfo(p));
                    }
                }
                foreach (Hediff h in p.health.hediffSet.hediffs)
                {
                    if (h is Hediff_Injury)
                    {
                        Hediff_Injury hi = (Hediff_Injury)h;
                        if (hi.IsOld())
                        {
                            //Log.Message(p.NameStringShort + " has new " + hi.Label + " on their " + hi.Part.def.label + "!");
                            bool isNew = false;
                            isNew = set.Add(new PawnPartProblem(p.thingIDNumber, hi.Part.def.label, hi.Label));
                            if (notify && isNew)
                            {
                                //Log.Message(p.NameStringShort + " has new " + hi.Label + " on their " + hi.Part.def.label + "!");
                                Find.LetterStack.ReceiveLetter("New Scar", p.NameStringShort + " has new " + hi.Label + " on their " + hi.Part.def.label + "!", LetterType.BadUrgent, new GlobalTargetInfo(p));
                            }
                        }
                    }
                }
            }
            //Log.Message("Hello from InjuryNotifier updateCollection END");
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
