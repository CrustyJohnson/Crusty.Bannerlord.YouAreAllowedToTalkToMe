using System;
using SandBox;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.ObjectSystem;

namespace Crusty.Bannerlord.YouAreAllowedToTalkToMe
{
    public class HeroPrisonerCampaignBehavior : CampaignBehaviorBase
    {
        public static bool IsMercenaryPrisoner { get; set; }


        public override void RegisterEvents() =>
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this,
                new Action<CampaignGameStarter>(OnSessionLaunched));
        

        public override void SyncData(IDataStore dataStore)
        {

        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            AddDialogs(campaignGameStarter);
            InformationManager.DisplayMessage(new InformationMessage("Crusty.HeroPrisonerBehavior Added!"));
        }


        public void AddDialogs(CampaignGameStarter starter)
        {
            IsMercenaryPrisoner = false;
            // Player calms and assuages the prisoner
            starter.AddDialogLine("prisoner_is_mercenary", "start", "hero_ask",
                "I miss my family...hopefully I'll live to see them again...[if:convo_dismayed]",
                OnConditionIsMercenaryPrisoner, null, 110);
            starter.AddDialogLine("prisoner_f2f_start", "start", "prisoner_f2f_startOut",
                "What the hell do you want??[if:convo_angry]",
                conversation_prisoner_f2f_on_condition, null);
            starter.AddPlayerLine("player_calm_down",
                "prisoner_f2f_startOut", "player_calm_down",
                "Calm down so we can talk for a moment!",
                prisoner_f2f_true,
                null);
            starter.AddPlayerLine("player_nevermind",
                "prisoner_f2f_startOut", "close_window",
                "Never mind...", new ConversationSentence.OnConditionDelegate(prisoner_f2f_true), null);
            starter.AddDialogLine("prisoner_acknowledges",
                "player_calm_down", "player_decides",
                "Alright...alright...what is it?[if:convo_grave]",
                new ConversationSentence.OnConditionDelegate(prisoner_f2f_true), null);

            //Player decides what to do with the prisoner
            starter.AddPlayerLine("player_decides_leave_prisoner",
                "player_decides", "close_window",
                "Actually, I'll come back later...",
                new ConversationSentence.OnConditionDelegate(prisoner_f2f_true), null);
            starter.AddPlayerLine("player_decides_rob_prisoner",
                "player_decides", "prisoner_gets_robbed",
                "Relieve yourself of your personal effects and remove yourself my sight, cur!",
                prisoner_gets_robbed_on_condition, null);
            starter.AddPlayerLine("player_decides_release_prisoner",
                "player_decides", "prisoner_gets_released",
                "You are no longer my prisoner, godspeed.", prisoner_f2f_true, null);
            starter.AddPlayerLine("player_decides_recruit_mercenary_prisoner",
                "player_decides", "prisoner_recruited",
                "If you wish to be free, perhaps you can help me out and join my party, if only for a while...",
                PlayerDecidesRecruitMercenaryPrisonerCondition, null);
            starter.AddPlayerLine("player_decides_fight",
                "player_decides", "prisoner_converted",
                "The only mercy you'll get is your equipment and a small lead. Run.",
                null, null);
            starter.AddPlayerLine("player_decides_convert",
                "player_decides", "persuasion_leave_faction_npc",
                "WHy do you serve a liege that allows his subjects to rot away in prison?.",
                null, null);



            //Decisions ending the dialogue

            starter.AddDialogLine("prisoner_recruited_end",
                "prisoner_recruited", "close_window",
                "A fair proposal...I suppose it is better than the alternative.[if:convo_stern]", prisoner_f2f_true,
                RecruitPrisonerConsequence);
            starter.AddDialogLine("prisoner_released_happy",
                "prisoner_gets_released", "close_window",
                "A change of heart? I shant forget this act of kindness.[if:convo_delighted]", prisoner_f2f_true,
                ReleasePrisoner);
            starter.AddDialogLine("prisoner_robbed_end",
                "prisoner_gets_robbed", "close_window",
                "You can't possibly be serious! I swear, you'll pay for this. [if:convo_furious]",
                prisoner_f2f_true,
                TakePrisonerEquipmentAndRelease);
            starter.AddDialogLine("prisoner_fought",
                "prisoner_fight", "close_window",
                "What!? [if:convo_surprised]", prisoner_f2f_true, FightPrisonerConsequence);


        }

        private void FightPrisonerConsequence()
        {
            AddHeroToPartyAction.Apply(Hero.OneToOneConversationHero, new MobileParty());
            GameMenu.ExitToLast();
            StartBattleAction.ApplyStartBattle(MobileParty.MainParty, Hero.OneToOneConversationHero.PartyBelongedTo);
        }

        //CONDITIONS
        private bool OnConditionIsMercenaryPrisoner()
        {
            try
            {
                Hero mercenary = Hero.OneToOneConversationHero;
                if (mercenary.PartyBelongedTo == Hero.MainHero.PartyBelongedTo && mercenary.IsPrisoner &&
                    IsMercenaryPrisoner == true)
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private bool PlayerDecidesRecruitMercenaryPrisonerCondition()
        {
            Hero hero = Hero.OneToOneConversationHero;
            Clan clan = hero.Clan;

            if (clan.IsMafia || clan.IsNeutralClan || clan.IsBanditFaction || clan.IsNomad)
            {
                return true;

            }

            if (clan.IsClanTypeMercenary && !clan.IsUnderMercenaryService)
            {
                return true;
            }
            else
            {
                return false;

            }
        }

        private bool prisoner_gets_robbed_on_condition()
        {
            try
            {
                if (Hero.OneToOneConversationHero.PartyBelongedToAsPrisoner.Owner == Hero.MainHero)
                {
                    return true;
                }
                else return false;
            }
            catch
            {
                return true;
            }
        }


        private bool prisoner_f2f_true()
        {
            return true;
        }

        // CONSEQUENCES
        protected void RecruitPrisonerConsequence()
        {
            Hero hero2 = Hero.OneToOneConversationHero;
            MobileParty party = MobileParty.MainParty;
            TroopRoster prisonRoster = party.PrisonRoster;
            AddHeroToPartyAction.Apply(hero2, party);
            party.PrisonRoster.RemoveTroop(hero2.CharacterObject);
            IsMercenaryPrisoner = true;

        }

        protected void TakePrisonerEquipmentAndRelease()
        {
            Hero hero1 = Hero.MainHero;
            Hero hero2 = Hero.OneToOneConversationHero;

            try
            {
                hero1.PartyBelongedTo.ItemRoster.AddToCounts(
                hero2.BattleEquipment.GetEquipmentFromSlot(EquipmentIndex.Cape), 1);
            }
            catch { }
            try
            {
                hero1.PartyBelongedTo.ItemRoster.AddToCounts(
                hero2.BattleEquipment.GetEquipmentFromSlot(EquipmentIndex.Gloves), 1);
            }
            catch { }

            try
            {
                hero1.PartyBelongedTo.ItemRoster.AddToCounts(
            hero2.BattleEquipment.GetEquipmentFromSlot(EquipmentIndex.Head), 1);

            }
            catch { }

            try
            {
                hero1.PartyBelongedTo.ItemRoster.AddToCounts(
            hero2.BattleEquipment.GetEquipmentFromSlot(EquipmentIndex.Leg), 1);
            }
            catch { }
            try
            {
                hero1.PartyBelongedTo.ItemRoster.AddToCounts(
                    hero2.BattleEquipment.GetEquipmentFromSlot(EquipmentIndex.Body), 1);
            }
            catch { }
            try
            {
                hero1.PartyBelongedTo.ItemRoster.AddToCounts(
                    hero2.BattleEquipment.GetEquipmentFromSlot(EquipmentIndex.Weapon0), 1);
            }
            catch { }
            try
            {
                hero1.PartyBelongedTo.ItemRoster.AddToCounts(
                    hero2.BattleEquipment.GetEquipmentFromSlot(EquipmentIndex.Weapon1), 1);
            }
            catch { }
            try
            {
                hero1.PartyBelongedTo.ItemRoster.AddToCounts(
                    hero2.BattleEquipment.GetEquipmentFromSlot(EquipmentIndex.Weapon2), 1);
            }
            catch { }
            try
            {
                hero1.PartyBelongedTo.ItemRoster.AddToCounts(
                    hero2.BattleEquipment.GetEquipmentFromSlot(EquipmentIndex.Weapon3), 1);
            }
            catch { }
            try
            {
                hero1.PartyBelongedTo.ItemRoster.AddToCounts(
                    hero2.BattleEquipment.GetEquipmentFromSlot(EquipmentIndex.Weapon4), 1);
            }
            catch { }
            try
            {
                hero1.PartyBelongedTo.ItemRoster.AddToCounts(
                    hero2.BattleEquipment.GetEquipmentFromSlot(EquipmentIndex.Horse), 1);
            }
            catch { }
            try
            {
                hero1.PartyBelongedTo.ItemRoster.AddToCounts(
                    hero2.BattleEquipment.GetEquipmentFromSlot(EquipmentIndex.HorseHarness), 1);
            }
            catch { }




            Equipment equipment = hero2.BattleEquipment;
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(Hero.MainHero, hero2, -25, true);
            EndCaptivityAction.ApplyByEscape(hero2);

        }

        protected void ReleasePrisoner()
        {
            Hero hero1 = Hero.MainHero;
            Hero hero2 = Hero.OneToOneConversationHero;
            ChangeRelationAction.ApplyPlayerRelation(hero2, +3, false, true);
            EndCaptivityAction.ApplyByReleasing(hero2);
        }

        private bool conversation_prisoner_f2f_on_condition() => Hero.OneToOneConversationHero != null &&
                                                                 Hero.OneToOneConversationHero.HeroState ==
                                                                 Hero.CharacterStates.Prisoner &&
                                                                 Campaign.Current.CurrentConversationContext !=
                                                                 ConversationContext.CapturedLord;

    }
}