﻿using RimWorld;
using Verse;

namespace BigAndSmall
{
    /// <summary>
    /// Just copy pasting this from the "Femboy" mod for the simple reason that "Femboy" is a too controversial term to have as a dependency.
    /// </summary>
    [DefOf]
    public static class BSDefs
    {
        //public static StatDef BS_FinalSizeMultiplier;
        public static StatDef BS_MaxNutritionFromSize;

        public static StatDef SM_BodySizeOffset;
        public static StatDef SM_Cosmetic_BodySizeOffset;
        public static StatDef SM_BodySizeMultiplier;
        public static StatDef SM_Cosmetic_BodySizeMultiplier;
        public static StatDef SM_HeadSize_Cosmetic;
        public static StatDef SM_Minimum_Learning_Speed;
        public static StatDef SM_Food_Need_Capacity;
        public static StatDef SM_AttackSpeed;
        public static StatDef SM_UnarmedAttackSpeed;
        public static StatDef SM_GrowthPointAccumulation;
        [MayRequire("RedMattis.BigSmall.Core")]
        public static StatDef BS_SoulPower;

        // Traits
        [MayRequire("RedMattis.BigSmall.Core")]
        public static TraitDef BS_Giant;
        [MayRequire("Ludeon.Rimworld.Biotech")]
        public static TraitDef BS_Gentle;
        public static TraitDef Cannibal;
        public static TraitDef Beauty;
        public static TraitDef SpeedOffset;
        public static TraitDef Tough;
        public static TraitDef Masochist;

        // Genes
        [MayRequire("Ludeon.Rimworld.Biotech")]
        public static GeneDef Robust;
        [MayRequire("Ludeon.Rimworld.Biotech")]
        public static GeneDef DiseaseFree;
        [MayRequire("Ludeon.Rimworld.Biotech")]
        public static GeneDef Ageless;
        [MayRequire("Ludeon.Rimworld.Biotech")]
        public static GeneDef Body_Androgynous;
        [MayRequire("Ludeon.Rimworld.Biotech")]
        public static GeneDef Body_MaleOnly;
        [MayRequire("Ludeon.Rimworld.Biotech")]
        public static GeneDef Body_FemaleOnly;
        [MayRequire("RedMattis.BigSmall.Core")]
        public static GeneDef BS_ReturningSoul;
        [MayRequire("RedMattis.BigSmall.Core")]
        public static GeneDef BS_Immortal;
        [MayRequire("RedMattis.BigSmall.Core")]
        public static GeneDef BS_AlienApperanceStandards;
        [MayRequire("RedMattis.BigSmall.Core")]
        public static GeneDef BS_AlienApperanceStandards_Lesser;

        // Hediffs
        public static HediffDef VU_SuccubusBond;
        public static HediffDef VU_SuccubusBond_Victim;
        public static HediffDef VU_WhiteRoseBite;
        public static HediffDef VU_WhiteRoseThrall;
        public static HediffDef VU_Euphoria;
        public static HediffDef VU_DraculVampirism;
        public static HediffDef VU_DraculAge;
        public static HediffDef VU_AnimalReturned;
        public static HediffDef VU_DraculAnimalVampirism;
        [MayRequire("RedMattis.BigSmall.Core")]
        public static HediffDef BS_LesserDeathless_Death;
        [MayRequire("RedMattis.BigSmall.Core")]
        public static HediffDef BS_BurnReturnDenial;
        [MayRequire("RedMattis.BigSmall.Core")]
        public static HediffDef BS_SoulPowerHediff;
        [MayRequire("RedMattis.BigSmall.Core")]
        public static HediffDef BS_IndestructibelApparel;

        // Weapons
        public static WeaponClassDef BS_GiantWeapon;

        public static MentalStateDef BS_LostManhunter;
        public static MentalStateDef BS_LostManhunterPermanent;

        // Damage
        public static DamageDef Arrow;
        [MayRequire("RedMattis.BigSmall.Core")]
        public static DamageDef BS_BiteDevourDmg;

        // Organs
        public static ThingDef Heart;
        public static ThingDef Lung;
        public static ThingDef Kidney;
        public static ThingDef Liver;

        public static ThingDef Filth_MachineBits;

        // Capacity
        public static PawnCapacityDef Metabolism;

        // ThoughtDefs
        public static ThoughtDef AteHumanlikeMeatDirectCannibal;
        public static ThoughtDef AteHumanlikeMeatDirect;

        [MayRequireIdeology]
        public static PreceptDef Cannibalism_Acceptable;

        [MayRequire("RedMattis.BigSmall.Core")]
        public static ThoughtDef BS_DroneDied;
        [MayRequire("RedMattis.BigSmall.Core")]
        public static ThoughtDef BS_DroneLost;

        // BodyPartGroupDefs
        public static BodyPartGroupDef Feet;

        // BodyParts
        public static BodyPartDef Tail;
        public static BodyPartDef BS_Wing;

        //public static ThingDef BS_MechHuman;
        //public static HediffDef BS_MechHumanRace;

        // Categories
        public static ThingCategoryDef BS_RobotCorpses;

        // Recipes
        public static RecipeDef BS_ShredRobot;
        public static RecipeDef BS_SmashRobot;
        //public static RecipeDef ButcherCorpseMechanoid;
        //public static RecipeDef SmashCorpseMechanoid;
        //public static RecipeDef ButcherCorpseFlesh;

        // Shaders
        public static ShaderTypeDef CutoutComplexBlend;



        public static IncidentDef StrangerInBlackJoin; // Def of the existing incident for easy acccess.
        //public static IncidentDef WomanInBlueJoin;

        static BSDefs()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(BSDefs));
        }

    }
}
