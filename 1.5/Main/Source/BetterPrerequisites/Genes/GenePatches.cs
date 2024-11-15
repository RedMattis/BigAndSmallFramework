﻿using BigAndSmall;
using BigAndSmall.SpecialGenes.Gender;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Verse;

namespace BetterPrerequisites
{

    [HarmonyPatch]
    public static class NotifyGenesChanges
    {
        [HarmonyPatch(typeof(Pawn_GeneTracker), "Notify_GenesChanged")]
        [HarmonyPostfix]
        public static void Notify_GenesChanged_Postfix(GeneDef addedOrRemovedGene, Pawn_GeneTracker __instance)
        {
            try
            {
                PGene.UpdateOverridenGenes(addedOrRemovedGene, __instance);
            }
            catch (Exception e)
            {
                Log.Warning(e.ToString());
            }

            // Update the Cache
            if (__instance?.pawn != null)
            {
                var cache = HumanoidPawnScaler.GetCache(__instance.pawn, scheduleForce: 1);
                GenderMethods.UpdatePawnHairAndHeads(__instance.pawn);

                if (__instance?.pawn?.Drawer?.renderer != null && __instance.pawn.Spawned)
                    __instance.pawn.Drawer.renderer.SetAllGraphicsDirty();

                Patch_NotifyGendesChanged_Gender.RunInPostfix(__instance.pawn, addedOrRemovedGene);
            }
        }

        [HarmonyPatch(typeof(Gene), nameof(Gene.OverrideBy))]
        [HarmonyPostfix]
        public static void Gene_OverrideBy_Patch(Gene __instance, Gene overriddenBy)
        {
            if (!BigSmall.performScaleCalculations) return;
            bool overriden = overriddenBy != null;
            Gene gene = __instance;
            if (gene != null && gene.pawn != null && gene.pawn.Spawned)
            {
                GeneEffectManager.GainOrRemovePassion(overriden, gene);

                GeneEffectManager.GainOrRemoveAbilities(overriden, gene);

                GeneEffectManager.ApplyForcedTraits(overriden, gene);


                if (gene?.pawn != null)
                {
                    HumanoidPawnScaler.GetCache(gene.pawn, scheduleForce: 1);
                }
            }
        }

        [HarmonyPatch(typeof(Gene), "PostRemove")]
        [HarmonyPostfix]
        public static void Gene_PostRemovePatch(Gene __instance)
        {
            if (__instance is PGene && !PawnGenerator.IsBeingGenerated(__instance.pawn) && __instance.Active)
            {
                GeneEffectManager.RefreshGeneEffects(__instance, activate: false);
            }
        }

        [HarmonyPatch(typeof(Gene), nameof(Gene.PostAdd))]
        [HarmonyPostfix]
        public static void Gene_PostAddPatch(Gene __instance)
        {
            if (__instance?.pawn?.genes != null)
            {
                Pawn pawn = __instance.pawn;
                // Get all other genes
                var genes = GeneHelpers.GetAllActiveGenes(__instance.pawn);

                // Check if active. This will trigger the checker for prerequisites.
                genes.Where(g => g is PGene pGene).Cast<PGene>().ToList().ForEach(pg => pg.ForceRun = true);

                foreach (var gene in genes)
                {
                    GeneEffectManager.RefreshGeneEffects(gene, activate: true);
                }
            }
        }
    }

    // Harmony class which postfixes the GetDescriptionFull method to insert a description which informs the user about what the sets of prerequisites, their type, and their contents in a human-readable way.
    [HarmonyPatch(typeof(GeneDef), "GetDescriptionFull")]
    public static class GeneDef_GetDescriptionFull
    {
        public static void Postfix(ref string __result, GeneDef __instance)
        {
            if (__instance.HasModExtension<ProductionGeneSettings>())
            {
                var geneExtension = __instance.GetModExtension<ProductionGeneSettings>();
                StringBuilder stringBuilder = new();

                stringBuilder.AppendLine(__result);
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("BS_ProductionTooltip".Translate(geneExtension.baseAmount, geneExtension.product.LabelCap.AsTipTitle(), geneExtension.frequencyInDays));

                __result = stringBuilder.ToString();
            }

            if (__instance.HasModExtension<GenePrerequisites>())
            {
                var geneExtension = __instance.GetModExtension<GenePrerequisites>();
                if (geneExtension.prerequisiteSets != null)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine(__result);
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine(("BP_GenePrerequisites".Translate() + ":").Colorize(ColoredText.TipSectionTitleColor));
                    foreach (var prerequisiteSet in geneExtension.prerequisiteSets)
                    {
                        if (prerequisiteSet.prerequisites != null)
                        {
                            stringBuilder.AppendLine();
                            stringBuilder.AppendLine(($"BP_{prerequisiteSet.type}".Translate() + ":").Colorize(GeneUtility.GCXColor));
                            foreach (var prerequisite in prerequisiteSet.prerequisites)
                            {
                                var gene = DefDatabase<GeneDef>.GetNamedSilentFail(prerequisite);
                                if (gene != null)
                                {
                                    stringBuilder.AppendLine(" - " + gene.LabelCap);
                                }
                                else
                                {
                                    stringBuilder.AppendLine($" - {prerequisite} ({"BP_GeneNotFoundInGame".Translate()})");
                                }
                            }

                        }
                    }
                    __result = stringBuilder.ToString();
                }
            }

            if (__instance.HasModExtension<GeneSuppressor_Gene>())
            {
                var suppressExtension = __instance.GetModExtension<GeneSuppressor_Gene>();
                if (suppressExtension.supressedGenes != null)
                {
                    StringBuilder stringBuilder = new();
                    stringBuilder.AppendLine(__result);
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine(("BP_GenesSuppressed".Translate() + ":").Colorize(ColoredText.TipSectionTitleColor));
                    foreach (var geneDefName in suppressExtension.supressedGenes)
                    {
                        var gene = DefDatabase<GeneDef>.GetNamedSilentFail(geneDefName);
                        if (gene != null)
                        {
                            stringBuilder.AppendLine(" - " + gene.LabelCap);
                        }
                        else
                        {
                            stringBuilder.AppendLine($" - {geneDefName} ({"BP_GeneNotFoundInGame".Translate()})");
                        }
                    }
                    __result = stringBuilder.ToString();
                }
            }

            if (__instance.HasModExtension<PawnExtension>())
            {
                var geneExt = __instance.GetModExtension<PawnExtension>();

                StringBuilder stringBuilder = new();
                stringBuilder.AppendLine(__result);

                if (geneExt.conditionals != null)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine(("BP_ActiveOnCondition".Translate() + ":").Colorize(ColoredText.TipSectionTitleColor));
                    foreach (var conditional in geneExt.conditionals)
                    {
                        stringBuilder.AppendLine(" - " + conditional.Label);
                    }
                }
                if (geneExt.sizeByAge != null)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLineTagged(("SizeOffsetByAge".Translate().CapitalizeFirst() + ":").Colorize(ColoredText.TipSectionTitleColor));
                    stringBuilder.AppendLine(geneExt.sizeByAge.Select((CurvePoint pt) => "PeriodYears".Translate(pt.x).ToString() + ": +" + pt.y.ToString()).ToLineList("  - ", capitalizeItems: true));
                }

                var effectorB = geneExt.GetAllEffectorDescriptions();
                stringBuilder.Append(effectorB);



                __result = stringBuilder.ToString();
            }
        }
    }



    
}