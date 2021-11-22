using AlephVault.Unity.Meetgard.Scopes.Authoring.Behaviours.Server;
using AlephVault.Unity.Meetgard.Types;
using GameMeanMachine.Unity.NetRose.Authoring.Behaviours.Server;
using GameMeanMachine.Unity.NetRose.Samples.Common.Types;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


namespace GameMeanMachine.Unity.NetRose
{
    namespace Samples
    {
        namespace Server
        {
            public class OwnableModelServerSide : NetRoseModelServerSide<Ownable, Ownable>
            {
                public ulong ConnectionId;

                private static Ownable Owned = new Ownable() { IsOwned = true };
                private static Ownable NotOwned = new Ownable() { IsOwned = false };

                protected void Awake()
                {
                    base.Awake();
                    OnAfterSpawned += OwnableModelServerSide_OnSpawned;
                    OnBeforeDespawned += OwnableModelServerSide_OnDespawned;
                }

                protected void Start()
                {
                    base.Start();
                }

                protected void OnDestroy()
                {
                    base.OnDestroy();
                    OnSpawned -= OwnableModelServerSide_OnSpawned;
                    OnDespawned -= OwnableModelServerSide_OnDespawned;
                }

                private async Task OwnableModelServerSide_OnSpawned()
                {
                    Debug.Log($"Spawned for Connection Id: {ConnectionId} at scope: {Scope.Id}");
                    var _ = Protocol.SendTo(ConnectionId, Scope.Id);
                }

                private async Task OwnableModelServerSide_OnDespawned()
                {
                    var _ = Protocol.SendToLimbo(ConnectionId);
                }

                protected override Ownable GetInnerFullData(ulong connectionId)
                {
                    return (connectionId == ConnectionId) ? Owned : NotOwned;
                }

                protected override Ownable GetInnerRefreshData(ulong connectionId, string context)
                {
                    return (connectionId == ConnectionId) ? Owned : NotOwned;
                }
            }
        }
    }
}
