using AlephVault.Unity.Binary;
using GameMeanMachine.Unity.NetRose.Types.Models;
using System.Threading.Tasks;


namespace GameMeanMachine.Unity.NetRose
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace Server
            {
                /// <summary>
                ///   This is a map object, which is synced, but also knows
                ///   its owner and continually syncs it across the scopes
                ///   it moves.
                /// </summary>
                public abstract class OwnedNetRoseModelServerSide<SpawnData, RefreshData> : NetRoseModelServerSide<OwnedModel<SpawnData>, RefreshData>, IServerOwned
                    where SpawnData : class, ISerializable, new()
                    where RefreshData : class, ISerializable, new()
                {
                    /// <summary>
                    ///   This is the owner of the object.
                    /// </summary>
                    private ulong Owner;
                    
                    protected void Awake()
                    {
                        base.Awake();
                        OnAfterSpawned += OwnedNetRoseModelServerSide_OnSpawned;
                        OnBeforeDespawned += OwnedNetRoseModelServerSide_OnDespawned;
                    }
                    
                    protected void OnDestroy()
                    {
                        base.OnDestroy();
                        OnSpawned -= OwnedNetRoseModelServerSide_OnSpawned;
                        OnDespawned -= OwnedNetRoseModelServerSide_OnDespawned;
                    }

                    private async Task OwnedNetRoseModelServerSide_OnSpawned()
                    {
                        var _ = Protocol.SendTo(Owner, Scope.Id);
                    }
                    
                    private async Task OwnedNetRoseModelServerSide_OnDespawned()
                    {
                        var _ = Protocol.SendToLimbo(Owner);
                    }

                    public void SetOwner(ulong connectionId)
                    {
                        Owner = connectionId;
                    }

                    public ulong GetOwner()
                    {
                        return Owner;
                    }
                }
            }
        }
    }
}
