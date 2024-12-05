﻿using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BigAndSmall
{
    [HarmonyPatch]
    public static class MechanicalColonistPatches
    {
        [HarmonyPatch(typeof(HealthUtility), "TryAnesthetize")]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Low)]
        public static bool TryAnesthetizePatch(Pawn pawn)
        {
            if (HumanoidPawnScaler.GetCacheUltraSpeed(pawn) is BSCache cache && cache.isMechanical)
            {
                return false;
            }
            return true;
        }

        //public ResolvedWound ChooseWoundOverlay(Hediff hediff)
        [HarmonyPatch(typeof(FleshTypeDef), nameof(FleshTypeDef.ChooseWoundOverlay))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Low)]
        public static bool ChooseWoundOverlayPatch(ref FleshTypeDef.ResolvedWound __result, FleshTypeDef __instance, Hediff hediff)
        {
            if (__instance != FleshTypeDefOf.Mechanoid && HumanoidPawnScaler.GetCacheUltraSpeed(hediff.pawn) is BSCache cache && cache.isMechanical)
            {
                __result = FleshTypeDefOf.Mechanoid.ChooseWoundOverlay(hediff);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(CompRottable), nameof(CompRottable.Active), MethodType.Getter)]
        [HarmonyPriority(int.MaxValue)]
        public static bool Deactivate_CompRottable(CompRottable __instance, ref bool __result)
        {
            if (__instance.parent is Corpse corpse &&
                HumanoidPawnScaler.GetCacheUltraSpeed(corpse.InnerPawn) is BSCache cache && cache.isMechanical)
            {
                __result = false;
                return false;
            }
            return true;
        }

        //[HarmonyPatch(typeof(ThoughtWorker_TranshumanistAppreciation), "CurrentSocialStateInternal", [typeof(Pawn), typeof(Pawn)])]
        //[HarmonyPriority(Priority.Low)]
        //public static class TranshumanistAppreciation_Patch
        //{
        //    public static void Postfix(ref ThoughtState __result, Pawn pawn, Pawn other)
        //    {
        //        if (HumanoidPawnScaler.GetCacheUltraSpeed(other) is BSCache cache && cache.isMechanical)
        //        {
        //            __result = ThoughtState.ActiveAtStage(99);
        //        }
        //    }
        //}

        //[HarmonyPatch(typeof(ThoughtWorker_BodyPuristDisgust), "CurrentSocialStateInternal", [typeof(Pawn), typeof(Pawn)])]
        //[HarmonyPriority(Priority.Low)]
        //public static class BodyPuristDisgust_Patch
        //{
        //    public static void Postfix(ref ThoughtState __result, Pawn pawn, Pawn other)
        //    {
        //        if (HumanoidPawnScaler.GetCacheUltraSpeed(other) is BSCache cache && cache.isMechanical)
        //        {
        //            __result = ThoughtState.ActiveAtStage(99);
        //        }
        //    }


        //}
        //[HarmonyPatch(typeof(ThoughtWorker_HasAddedBodyPart), "CurrentStateInternal", [typeof(Pawn)])]
        //[HarmonyPriority(Priority.Low)]
        //public static class HasAddedBodyPart_Patch
        //{
        //    public static void Postfix(ref ThoughtState __result, Pawn pawn)
        //    {
        //        if (HumanoidPawnScaler.GetCacheUltraSpeed(pawn) is BSCache cache && cache.isMechanical)
        //        {
        //            __result = ThoughtState.ActiveAtStage(99);
        //        }
        //    }
        //}
        [HarmonyPatch(typeof(GeneUtility), nameof(GeneUtility.AddedAndImplantedPartsWithXenogenesCount), [typeof(Pawn)])]
        [HarmonyPriority(Priority.Low)]
        public static class AddedAndImplantedPartsWithXenogenesCount_Patch
        {
            public static void Postfix(ref int __result, Pawn pawn)
            {
                if (HumanoidPawnScaler.GetCacheUltraSpeed(pawn) is BSCache cache && cache.isMechanical)
                {
                    __result += 2;
                }
            }
        }


        public static Dictionary<BodyDef, Dictionary<BodyPartDef, List<BodyPartRecord>>> cachedRecordsPerPartDefDefPerBodydef = []; // Haha
        [HarmonyPatch(typeof(BodyDef), nameof(BodyDef.GetPartsWithDef))]
        [HarmonyPriority(Priority.Low)]
        [HarmonyPostfix]
        public static void GetPartsWithDef_Postfix(ref IEnumerable<BodyPartRecord> __result, BodyDef __instance, BodyPartDef def)
        {
            if (HumanPatcher.partImportsFromDictReverse.TryGetValue(def, out var partDefList))
            {
                if (!cachedRecordsPerPartDefDefPerBodydef.TryGetValue(__instance, out var cachedResult))
                {
                    cachedResult = [];
                    cachedRecordsPerPartDefDefPerBodydef[__instance] = cachedResult;
                }

                if (!cachedResult.TryGetValue(def, out var cachedParts))
                {
                    cachedParts = [];
                    foreach (var partDef in partDefList)
                    {
                        for (int i = 0; i < __instance.AllParts.Count; i++)
                        {
                            BodyPartRecord bodyPartRecord = __instance.AllParts[i];
                            if (bodyPartRecord.def == partDef && !cachedParts.Contains(bodyPartRecord))
                            {
                                cachedParts.Add(bodyPartRecord);
                            }
                        }
                    }
                    cachedRecordsPerPartDefDefPerBodydef[__instance][def] = cachedParts;
                }

                var resultList = __result.ToList();
                resultList.AddRange(cachedParts);
                __result = resultList;
            }
        }
    }
}
