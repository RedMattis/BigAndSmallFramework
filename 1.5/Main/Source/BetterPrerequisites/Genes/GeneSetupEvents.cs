﻿using BetterPrerequisites;
using HarmonyLib;
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
    public static class GeneSetupHarmonyPatches
    {

        [HarmonyPatch(typeof(PawnGenerator), "GenerateGenes")]
        [HarmonyPostfix]
        public static void GenerateGenes_Postfix(Pawn pawn, XenotypeDef xenotype, PawnGenerationRequest request)
        {
            if (xenotype.GetForcedRace() is (ThingDef forcedRace, bool force))
            {
                try
                {
                    pawn.SwapThingDef(forcedRace, state: true, force: force);
                }
                catch (Exception e)
                {
                    Log.Error($"Error while trying to swap {pawn.Name} to {forcedRace.defName} during GenerateGenes step: {e.Message}");
                }
            }
        }

    }
}
