using UnityEngine;
using AlephVault.Unity.Meetgard.Scopes.Authoring.Behaviours.Client;
using AlephVault.Unity.Meetgard.Types;
using GameMeanMachine.Unity.NetRose.Samples.Common.Types;
using GameMeanMachine.Unity.NetRose.Authoring.Behaviours.Client;
using GameMeanMachine.Unity.NetRose.Authoring.Behaviours.Server;
using GameMeanMachine.Unity.NetRose.Types.Models;


namespace GameMeanMachine.Unity.NetRose
{
    namespace Samples
    {
        namespace Client
        {
            public class OwnableModelClientSide : OwnedNetRoseModelClientSide<Nothing, Nothing>
            {
                protected override void InflateOwnedFrom(Nothing fullData)
                {
                }
                
                protected override void UpdateFrom(Nothing refreshData)
                {
                }
            }
        }
    }
}
