using AlephVault.Unity.Meetgard.Scopes.Authoring.Behaviours.Client;
using AlephVault.Unity.Meetgard.Types;
using GameMeanMachine.Unity.NetRose.Authoring.Behaviours.Client;


namespace GameMeanMachine.Unity.NetRose
{
    namespace Samples
    {
        namespace Client
        {
            public class EmptyModelClientSide : NetRoseModelClientSide<Nothing, Nothing>
            {
                protected override void InflateFrom(Nothing fullData) {}

                protected override void UpdateFrom(Nothing refreshData) {}
            }
        }
    }
}
