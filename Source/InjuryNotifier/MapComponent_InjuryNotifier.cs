using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace InjuryNotifier;

public class MapComponent_InjuryNotifier : MapComponent
{
    public bool firstRun = true;
    public HashSet<PawnPartProblem> set = new HashSet<PawnPartProblem>();
    public int slowDown;

    public MapComponent_InjuryNotifier(Map map) : base(map)
    {
        LongEventHandler.QueueLongEvent(ensureComponentExists, null, false, null);
    }

    public static void ensureComponentExists()
    {
        foreach (var m in Find.Maps)
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
        if (slowDown <= 100)
        {
            return;
        }

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

    public void updateCollection(bool notify)
    {
        //Log.Message("Hello from InjuryNotifier updateCollection BEGIN");
        foreach (var p in map.mapPawns.FreeColonistsAndPrisonersSpawned)
        {
            foreach (var hmp in p.health.hediffSet.GetMissingPartsCommonAncestors())
            {
                //Log.Message(p.NameStringShort + " lost " + hmp.Part.def.label + " due to " + hmp.Label + "!");
                var isNew = set.Add(
                    new PawnPartProblem(p.thingIDNumber, hmp.Part.def.label, trimInjuryLabel(hmp.Label)));
                if (notify && isNew)
                {
                    //Log.Message(p.NameStringShort + " lost " + hmp.Part.def.label + " due to " + hmp.Label + "!");
                    Find.LetterStack.ReceiveLetter("Lost Part",
                        p.Name.ToStringShort + " lost their " + hmp.Part.def.label + " due to " +
                        trimInjuryLabel(hmp.Label) + "!", LetterDefOf.NegativeEvent, new GlobalTargetInfo(p));
                }
            }

            foreach (var h in p.health.hediffSet.hediffs)
            {
                if (h is not Hediff_Injury hi)
                {
                    continue;
                }

                if (!hi.IsPermanent())
                {
                    continue;
                }

                //Log.Message(p.NameStringShort + " has new " + hi.Label + " on their " + hi.Part.def.label + "!");
                var isNew = set.Add(new PawnPartProblem(p.thingIDNumber, hi.Part.def.label,
                    trimInjuryLabel(hi.Label)));
                if (notify && isNew)
                {
                    //Log.Message(p.NameStringShort + " has new " + hi.Label + " on their " + hi.Part.def.label + "!");
                    Find.LetterStack.ReceiveLetter("New Scar",
                        p.Name.ToStringShort + " has new " + trimInjuryLabel(hi.Label) + " on their " +
                        hi.Part.def.label + "!", LetterDefOf.NegativeEvent, new GlobalTargetInfo(p));
                }
            }
        }
        //Log.Message("Hello from InjuryNotifier updateCollection END");
    }

    public static string trimInjuryLabel(string s)
    {
        var leftParenIndex = s.IndexOf(" (");
        if (leftParenIndex > 0)
        {
            return s.Substring(0, leftParenIndex);
        }

        return s;
    }
}