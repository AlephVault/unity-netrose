using AlephVault.Unity.Meetgard.Types;
using AlephVault.Unity.Binary.Wrappers;
using AlephVault.Unity.NetRose.Authoring.Behaviours.Server;
using AlephVault.Unity.NetRose.Types.Models;


namespace AlephVault.Unity.NetRose
{
    namespace Samples
    {
        namespace Server
        {
            public class OwnableModelServerSide : OwnedNetRoseModelServerSide<Nothing, Nothing>
            {
                public float LastCommandTime = 0;

                private static OwnedModel<Nothing> Owned = OwnedModel<Nothing>.AsOwned(Nothing.Instance);
                private static OwnedModel<Nothing> NotOwned = OwnedModel<Nothing>.AsNotOwned(Nothing.Instance);

                protected override OwnedModel<Nothing> GetInnerFullData(ulong connectionId)
                {
                    return GetOwner() == connectionId ? Owned : NotOwned;
                }

                protected override Nothing GetInnerRefreshData(ulong connectionId, string context)
                {
                    return Nothing.Instance;
                }
            }
        }
    }
}
