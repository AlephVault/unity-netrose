using AlephVault.Unity.Meetgard.Scopes.Authoring.Behaviours.Client;
using AlephVault.Unity.Meetgard.Types;
using AlephVault.Unity.NetRose.Authoring.Behaviours.Client;


namespace AlephVault.Unity.NetRose
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
