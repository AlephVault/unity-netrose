using AlephVault.Unity.Meetgard.Scopes.Authoring.Behaviours.Server;
using AlephVault.Unity.Meetgard.Types;
using GameMeanMachine.Unity.NetRose.Authoring.Behaviours.Server;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameMeanMachine.Unity.NetRose
{
    namespace Samples
    {
        namespace Server
        {
            public class EmptyModelServerSide : NetRoseModelServerSide<Nothing, Nothing>
            {
                Nothing nothing = new Nothing();

                protected override Nothing GetInnerFullData(ulong connectionId)
                {
                    return nothing;
                }

                protected override Nothing GetInnerRefreshData(ulong connectionId, string context)
                {
                    return nothing;
                }
            }
        }
    }
}
