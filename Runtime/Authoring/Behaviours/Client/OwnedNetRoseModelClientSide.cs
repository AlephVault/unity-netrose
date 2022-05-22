using AlephVault.Unity.Binary;
using GameMeanMachine.Unity.NetRose.Types.Models;
using UnityEngine;


namespace GameMeanMachine.Unity.NetRose
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace Client
            {
                /// <summary>
                ///   This is a map object, which is synced, but also knows
                ///   whether it is owned or not, and is continually
                ///   refreshed from the server.
                /// </summary>
                public abstract class OwnedNetRoseModelClientSide<SpawnData, RefreshData> : NetRoseModelClientSide<OwnedModel<SpawnData>, RefreshData>, IClientOwned
                    where SpawnData : class, ISerializable, new()
                    where RefreshData : class, ISerializable, new()
                {
                    private bool isOwned;
                    private Camera camera;
                    
                    /// <summary>
                    ///   Sets the ownership and delegates the call.
                    ///   Also, sets the camera triggers the update.
                    /// </summary>
                    /// <param name="fullData">The full data, including ownership</param>
                    protected override void InflateFrom(OwnedModel<SpawnData> fullData)
                    {
                        isOwned = fullData.Owned;
                        PrincipalObjectsNetRoseProtocolClientSide protocol = 
                            Protocol.GetComponent<PrincipalObjectsNetRoseProtocolClientSide>();
                        camera = protocol != null ? protocol.GetCamera() : Camera.main;
                        InflateOwnedFrom(fullData.Data);
                        Update();
                    }

                    /// <summary>
                    ///   Inflates the owned data, not counting the ownership.
                    ///   Also, this is done before the update.
                    /// </summary>
                    /// <param name="fullData">The full data, without ownership</param>
                    protected abstract void InflateOwnedFrom(SpawnData fullData);

                    protected override void UpdateFrom(RefreshData refreshData)
                    {
                        UpdateOwnedFrom(refreshData);
                        Update();
                    }

                    /// <summary>
                    ///   Updates the owned data. This is done before the update.
                    /// </summary>
                    /// <param name="refreshData">The refresh data</param>
                    protected abstract void UpdateOwnedFrom(RefreshData refreshData);

                    private void Update()
                    {
                        if (isOwned && camera) camera.transform.position = new Vector3(
                            transform.position.x, transform.position.y, -10
                        );
                    }

                    /// <summary>
                    ///   Returns whether the object is owned by the current connection.
                    /// </summary>
                    public bool IsOwned()
                    {
                        return isOwned;
                    }
                }
            }
        }
    }
}
