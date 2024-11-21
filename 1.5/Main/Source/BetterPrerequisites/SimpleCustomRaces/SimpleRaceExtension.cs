﻿using BetterPrerequisites;
using BigAndSmall.FilteredLists;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BigAndSmall
{

    public class RaceExtension : DefModExtension
    {
        // Can be used to whitelist/blacklist recipes.
        // Note that they still need to have the valid parts.


        protected HediffDef raceHediff = null;

        // The list version was made for "merged" races, but either can be used.
        private List<HediffDef> raceHediffList = [];
        public float? femaleGenderChance = null;

        public List<ThingDef> isFusionOf = null;
        public RomanceTags romanceTags = null;

        //public List<HediffDef> RaceHediffs => raceHediff == null ? raceHediffList : [.. raceHediffList, raceHediff];

        // Get Set
        public List<HediffDef> RaceHediffs
        {
            get => raceHediff == null ? raceHediffList : [.. raceHediffList, raceHediff];
            private set
            {
                if (value.Count > 0)
                {
                    raceHediff = null;
                    raceHediffList = value;
                }
                else
                {
                    raceHediff = null;
                    raceHediffList = [];
                }
            }
        }

        public List<PawnExtension> PawnExtensionOnRace => ModExtHelper.ExtensionsOnDefList<PawnExtension, HediffDef>(RaceHediffs);

        // Merge with FilterListSet<T> MergeFilters<T>'s extension methods.
        public FilterListSet<RecipeDef> SurgeryRecipes => PawnExtensionOnRace
            .Where(pe => pe.surgeryRecipes != null)
            .Select(pe => pe.surgeryRecipes)
            .Aggregate(new FilterListSet<RecipeDef>(), (acc, x) => acc.MergeFilters(x));
        //.SelectMany(pe => pe.surgeryRecipes).ToList();

        public RaceExtension() { }
        public RaceExtension(List<RaceExtension> sources)
        {
            if (sources.Count > 0)
            {
                RaceHediffs = sources.Where(other=>other.RaceHediffs != null).SelectMany(other => other.RaceHediffs).ToList();
                
                var fGenderChance = sources.Where(other => other.femaleGenderChance != null).Select(other => other.femaleGenderChance).ToList();
                if (fGenderChance.Count > 0) { femaleGenderChance = fGenderChance.Average(); }
            }
        }

        public void ApplyTrackerIfMissing(Pawn pawn)
        {
            if (TrackerMissing(pawn))
            {
                ApplyHediffToPawn(pawn);
            }
        }
        public bool TrackerMissing(Pawn pawn)
        {
            var raceHediffList = new List<HediffDef>();
            foreach (var raceExtensions in pawn.def.GetRaceExtensions())
            {
                raceHediffList.AddRange(raceExtensions.RaceHediffs);
            }
            if (raceHediffList.Count == 0) return false;

            return raceHediffList.Any(rh => !pawn.health.hediffSet.HasHediff(rh));

            // Check if the raceHediff is not null and if the pawn has the raceHediff.
            //return raceHediff != null && !pawn.health.hediffSet.HasHediff(raceHediff);
        }
        private void ApplyHediffToPawn(Pawn pawn)
        {
            if (RaceHediffs.Count > 0)
            {
                // Remove all other RaceTracker Hediffs
                RemoveOldRaceTrackers(pawn);

                // Ensure the raceDef is of the "RaceTracker" class or a subclass thereof.
                foreach (var raceHediff in RaceHediffs)
                {
                    if (raceHediff.hediffClass == typeof(RaceTracker))
                    {
                        pawn.health.AddHediff(raceHediff);
                    }
                    else
                    {
                        Log.Error($"{pawn}'s raceDef needs to be a {nameof(RaceTracker)} or subclass thereof.");
                    }
                    // Ensure the Hediff has a RaceCompProps component
                    if (raceHediff.HasComp(typeof(HediffComp_Race))) { }
                    else { Log.Error($"{pawn}'s raceDef needs to have a {nameof(HediffComp_Race)} component."); }

                    pawn.health.AddHediff(raceHediff);
                }
            }
            else
            {
                Log.Error($"{pawn} has a BigAndSmall.RaceExtension without an associated raceDef!");
            }
        }

        //public void SwapToThisRace(Pawn pawn, bool force = false)
        //{
        //    if (RaceHediffs.Count > 0)
        //    {
        //        RaceMorpher.SwapThingDef(pawn, pawn.def, false, force: force, priority: 100);
        //    }
        //    else { Log.Error($"{pawn} has a BigAndSmall.RaceExtension without an associated raceDef!"); }
        //}

        public static void RemoveOldRaceTrackers(Pawn pawn)
        {
            var oldRaceTrackers = pawn.health?.hediffSet?.hediffs?.Where(h => h is RaceTracker);
            if (oldRaceTrackers == null) return;

            var extensions = ModExtHelper.GetHediffExtensions<PawnExtension>(pawn, parentWhitelist: [typeof(RaceTracker)]);

            var ort = oldRaceTrackers.ToList();
            for (int idx = ort.Count - 1; idx >= 0; idx--)
            {
                Hediff hediff = ort[idx];
                if (hediff is RaceTracker)
                {
                    pawn.health.hediffSet.hediffs.Remove(hediff);
                }
            }

            List<GeneDef> genesThatCanBeRemoved = extensions.SelectMany(x => x.genesDependentOnRace).ToList();
            List<TraitDef> traitsThatCanBeRemoved = extensions.SelectMany(x => x.traitsDependentOnRace).ToList();
            // Remove all forced traits, hediffs and genes.
            foreach (var ext in extensions)
            {
                if (ext.forcedHediffs != null)
                {
                    foreach (var hediff in ext.forcedHediffs)
                    {
                        if (pawn.health.hediffSet.HasHediff(hediff))
                        {
                            pawn.health.hediffSet.hediffs.Remove(pawn.health.hediffSet.GetFirstHediffOfDef(hediff));
                        }
                    }
                }
                HashSet<GeneDef> genesToRemove = [.. ext.forcedEndogenes ?? ([]), .. ext.forcedXenogenes ?? ([]), .. ext.immutableEndogenes ?? ([])];

                foreach (var gene in genesToRemove.Where(genesThatCanBeRemoved.Contains))
                {
                    if (pawn.genes.GenesListForReading.Any(g => g.def == gene))
                    {
                        pawn.genes.RemoveGene(pawn.genes.GenesListForReading.First(g => g.def == gene));
                    }
                }
                if (ext.forcedTraits != null)
                {
                    foreach (var trait in ext.forcedTraits.Where(traitsThatCanBeRemoved.Contains))
                    {
                        if (pawn.story.traits.HasTrait(trait))
                        {
                            pawn.story.traits.allTraits.Remove(pawn.story.traits.GetTrait(trait));
                        }
                    }
                }
            }
        }
    }

}
