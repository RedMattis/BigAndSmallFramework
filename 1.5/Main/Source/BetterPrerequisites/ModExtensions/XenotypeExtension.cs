﻿using BigAndSmall;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BetterPrerequisites
{
    public class XenotypeExtension : DefModExtension
    {
        public float morphWeight = 1;
        public bool morphIgnoreGender = false;
        public ThingDef setRace = null;
        public bool forceRace = false;
        //public int? maxAge = null;
        //public Gender? morphGender = null;

        public List<List<string>> genePickPriority = null;

        //public class GenePicker
        //{
        //    public List<string> priority = new();
        //}
    }

    public static class XenoTypeDefExtensions
    {
        public static float GetMorphWeight(this XenotypeDef def)
        {
            if (def.HasModExtension<XenotypeExtension>())
            {
                return def.GetModExtension<XenotypeExtension>().morphWeight;
            }
            return 1;
        }

        public static (ThingDef thing, bool force) GetForcedRace(this XenotypeDef def)
        {
            if (def.HasModExtension<XenotypeExtension>())
            {
                return (def.GetModExtension<XenotypeExtension>().setRace, def.GetModExtension<XenotypeExtension>().forceRace);
            }
            return (null, false);
        }

        public static bool TrySwapToXenotypeThingDef(this Pawn pawn)
        {
            if (pawn?.genes?.Xenotype is XenotypeDef xeno && xeno.GetForcedRace() is (ThingDef forcedRace, bool force))
            {
                try
                {
                    pawn.SwapThingDef(forcedRace, state: true, force: force);
                    return true;
                }
                catch (Exception e)
                {
                    Log.Error($"Error while trying to swap {pawn.Name} to {forcedRace.defName} during GenerateGenes step: {e.Message}");
                }
            }
            return false;
        }
    }

}
