﻿namespace BigAndSmall
{
    public static class LamiaAttack
    {
        

        //private static void KillTarget(Pawn attacker, Pawn victim)
        //{
        //    var killThirds = attacker.needs?.TryGetNeed<Need_KillThirst>();
        //    if (killThirds != null)
        //    {
        //        killThirds.CurLevelPercentage = 1;
        //    }

        //    DamageInfo dinfo = new DamageInfo(new DamageDef { deathMessage= "{0} was eaten" }, 0, instigator: attacker, instigatorGuilty: true, intendedTarget: victim);

        //    if (!victim.IsWildMan())
        //    {
        //        if (attacker.Faction == Faction.OfPlayer
        //            && (!PrisonBreakUtility.IsPrisonBreaking(victim) && !SlaveRebellionUtility.IsRebelling(victim) && !victim.IsSlaveOfColony && !victim.IsPrisoner))
        //        {
        //            int goodwillChange = -30;
        //            Faction.OfPlayer.TryAffectGoodwillWith(victim.Faction, goodwillChange, canSendMessage: true, !victim.Faction.temporary, HistoryEventDefOf.AttackedMember, victim);
        //        }
        //    }

        //    //victim.TakeDamage(dinfo);
        //    victim.Kill(dinfo);
        //    if (MakeCorpse_Patch.corpse != null)
        //    {
        //        MakeCorpse_Patch.corpse.Destroy();
        //        MakeCorpse_Patch.corpse = null;
        //    }

        //    var rawCannibal = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDefOf.AteHumanlikeMeatDirectCannibal);
        //    attacker.needs.mood.thoughts.memories.TryGainMemory(rawCannibal);
        //}
    }
}
