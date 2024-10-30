﻿using BetterPrerequisites;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BigAndSmall
{
    [HarmonyPatch]
    public static class PregnancyPatches
    {

        public static bool disableBirthPatch = false;
        public static List<GeneDef> newBabyGenes = null;
        public static int? babyStartAge = null;

        public static List<Pawn> parents = new List<Pawn>();

        //[HarmonyPatch(typeof(Hediff_Pregnant),
        //    nameof(Hediff_Pregnant.DoBirthSpawn),
        //    new Type[] { typeof(Pawn), typeof(Pawn) }
        //    )]
        //[HarmonyPatch(typeof(PregnancyUtility),
        //    nameof(PregnancyUtility.ApplyBirthOutcome))]
        //[HarmonyPrefix]

        public static void ApplyPatches()
        {
            BSCore.harmony.Patch(AccessTools.Method(typeof(PregnancyUtility), name: nameof(PregnancyUtility.ApplyBirthOutcome_NewTemp), new Type[]
            {
                typeof(RitualOutcomePossibility),
                typeof(float),
                typeof(Precept_Ritual),
                typeof(List<GeneDef>),
                typeof(Pawn),
                typeof(Thing),
                typeof(Pawn),
                typeof(Pawn),
                typeof(LordJob_Ritual),
                typeof(RitualRoleAssignments),
                typeof(bool)
            }), prefix: new HarmonyMethod(typeof(PregnancyPatches), nameof(ApplyBirthOutcome_NewTemp_Prefix)));
        }
        public static bool ApplyBirthOutcome_NewTemp_Prefix(RitualOutcomePossibility outcome, float quality, Precept_Ritual ritual, List<GeneDef> genes, Pawn geneticMother, Thing birtherThing, Pawn father, Pawn doctor, LordJob_Ritual lordJobRitual, RitualRoleAssignments assignments, bool preventLetter)
        {
            // Check if the pawn has genes. If not, just let the regular method run.
            if (disableBirthPatch || geneticMother?.genes == null)
            {
                return true;
            }
            // Get the gene mod extension
            var activeGenes = GeneHelpers.GetAllActiveGenes(geneticMother);
            List<PawnExtension> geneExts = activeGenes
                    .Where(x => x?.def?.modExtensions != null && x.def.modExtensions.Any(y => y.GetType() == typeof(PawnExtension)))?
                    .Select(x => x.def.GetModExtension<PawnExtension>()).ToList();

            // If there are no gene extensions, just let the regular method run.
            if (geneExts == null || geneExts.Count == 0)
            {
                return true;
            }

            // Check if the mother has the gene that makes her give birth to multiple children (babyBirthCount).
            List<int> babyCountList = geneExts.FirstOrDefault(x => x.babyBirthCount != null)?.babyBirthCount;
            int babiesToSpawn = 1;
            if (babyCountList != null)
            {
                babiesToSpawn = babyCountList.RandomElement();
            }

            disableBirthPatch = true;

            babyStartAge = geneExts.FirstOrDefault(x => x.babyStartAge != null)?.babyStartAge ?? null;

            parents = new List<Pawn> { geneticMother, father }.Where(x=>x != null).ToList();
            for (int i = 0; i < babiesToSpawn; i++)
            {
                if (ModsConfig.IsActive("RedMattis.BetterGeneInheritance") && i > 0)
                {
                    // Invoke the "BGInheritance.BGI_HarmonyPatches.GetChildGenes" method which gives us new genes.
                    newBabyGenes = (List<GeneDef>)AccessTools.Method("BGInheritance.BGI_HarmonyPatches:GetChildGenes").Invoke(null, new object[] { geneticMother, father });
                }
                PregnancyUtility.ApplyBirthOutcome_NewTemp(outcome, quality, ritual, genes, geneticMother, birtherThing, father, doctor, lordJobRitual, assignments, preventLetter);
                newBabyGenes = null;
            }
            parents.Clear();
            babyStartAge = null;
            disableBirthPatch = false;
            return false;
        }

        [HarmonyPatch(
            typeof(PawnGenerator),
            nameof(PawnGenerator.GeneratePawn),
            new Type[] { typeof(PawnGenerationRequest) })
            ]
        [HarmonyPostfix]
        public static void GeneratePawnPostfix(Pawn __result, PawnGenerationRequest request)
        {

            //Log.Message($"[DEBUG] Running GeneratePawnPostfix for baby {__result.Name}");
            Pawn baby = __result;
            if (newBabyGenes != null)
            {
                //Log.Message($"[DEBUG] Setting baby genes to {newBabyGenes.Count} new genes.");

                baby.genes.Endogenes.Clear();
                baby.genes.Xenogenes.Clear();
                foreach (var gene in newBabyGenes)
                {
                    baby.genes.AddGene(gene, false);
                }
            }
            if (parents.Count > 0)
            {
                List<(Pawn pawn, float score)> parentScores = new();
                foreach (var parent in parents.Where(x => x.genes?.Xenotype != null))
                {
                    var babyGeneDefs = baby.genes.GenesListForReading.Select(x => x.def);
                    var parentXeno = parent.genes.Xenotype;
                    var parentGenes = parentXeno.genes;
                    bool xenoGenes = parentXeno.inheritable;
                    // Check is baby has all of the parent's xenotype genes.
                    float score = parentGenes.Sum(x => babyGeneDefs.Contains(x) ? 1 : 0) / (float)parentGenes.Count;
                    parentScores.Add((parent, score));
                }
                if (parentScores.Count > 0)
                {
                    var (parent, score) = parentScores.OrderByDescending(x => x.score).First();
                    if (score > 0.8f)
                    {
                        baby.genes.SetXenotypeDirect(parent.genes.Xenotype);

                        // This sometimes doesn't help because Rimworld forces that "HYBRID" xenotype on them.
                        //Log.Message($"[PregnancyPatches DEBUG] Set baby's xenotype to {parent.genes.Xenotype.LabelCap} with a score of {score}");
                    }
                }
            }
            if (babyStartAge != null)
            {
                baby.ageTracker.AgeBiologicalTicks = (long)(babyStartAge * 3600000);
            }

            __result = baby;
        }
    }
}