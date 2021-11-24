using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crusty.Bannerlord.YouAreAllowedToTalkToMe
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();

        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();

        }
        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            base.OnGameStart(game, gameStarter);
            if (!(game.GameType is Campaign))
                return;
            CampaignGameStarter campaignGameStarter = (CampaignGameStarter)gameStarter;
            this.AddBehaviors(campaignGameStarter);


        }

        private void AddBehaviors(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddBehavior(new HeroPrisonerCampaignBehavior());
            InformationManager.DisplayMessage(new InformationMessage("HeroPrisonerCampaignBehavior Added!"));

        }
    }
}