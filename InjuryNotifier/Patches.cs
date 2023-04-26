using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace InjuryNotifier
{
    [StaticConstructorOnStartup]
    class Main
    {
        static Main()
        {
            var harmony = new Harmony("com.github.harmony.rimworld.maarx.injurynotifier");
            Log.Message("Hello from Harmony in scope: com.github.harmony.rimworld.maarx.injurynotifier");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    //Verse.Pawn_HealthTracker
    //public void AddHediff(Hediff hediff, BodyPartRecord part = null, DamageInfo? dinfo = default(DamageInfo?), DamageWorker.DamageResult result = null)
    [HarmonyPatch(typeof(Pawn_HealthTracker))]
    [HarmonyPatch("AddHediff")]
    [HarmonyPatch(new Type[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageWorker.DamageResult) })]
    public class Patch_Pawn_HealthTracker_AddHediff
    {
        static void Postfix(Pawn_HealthTracker __instance, Hediff hediff, BodyPartRecord part, DamageInfo? dinfo, DamageWorker.DamageResult result)
        {
            Pawn p = hediff.pawn;
            if (p.IsColonistPlayerControlled || (p.guest != null && p.guest.IsPrisoner))
            {
                Hediff_Injury hi = hediff as Hediff_Injury;
                if (hi != null && hi.IsPermanent())
                {
                    Find.LetterStack.ReceiveLetter("New Scar", p.Name.ToStringShort + " has new " + trimInjuryLabel(hi.Label) + " on their " + hi.Part.def.label + "!", RimWorld.LetterDefOf.NegativeEvent, new GlobalTargetInfo(p));
                }
                Hediff_MissingPart hmp = hediff as Hediff_MissingPart;
                if (hmp != null)
                {
                    Find.LetterStack.ReceiveLetter("Lost Part", p.Name.ToStringShort + " lost their " + hmp.Part.def.label + " due to " + trimInjuryLabel(hmp.Label) + "!", RimWorld.LetterDefOf.NegativeEvent, new GlobalTargetInfo(p));
                }
            }
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
}
