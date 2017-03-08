using System.Collections.Generic;
using Verse;

namespace InjuryNotifier
{
    public class InjuryNotifier : MapComponent
    {
        public int slowDown = 0;
        public HashSet<PawnPartProblem> set = new HashSet<PawnPartProblem>();

        public InjuryNotifier(Map map) : base(map)
        {
            //Log.Message("Hello from InjuryNotifier InjuryNotifier");
            updateCollection(true);
        }

        public override void MapComponentTick()
        {
            //Log.Message("Hello from InjuryNotifier MapComponentTick");
            slowDown++;
            if (slowDown > 100)
            {
                slowDown = 0;
                updateCollection(true);
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
                        Log.Message(p.NameStringShort + " lost " + hmp.Part.def.label + " due to " + hmp.Label + "!");
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
                                Log.Message(p.NameStringShort + " has new " + hi.Label + " on their " + hi.Part.def.label + "!");
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
