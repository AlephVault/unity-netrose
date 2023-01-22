﻿using AlephVault.Unity.Binary;
using GameMeanMachine.Unity.NetRose.Types.Models;
using GameMeanMachine.Unity.WindRose.Types;
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

                    /// <summary>
                    ///   Returns whether the object is optimistic or not. This should
                    ///   be constant per class (or, at least, per prefab), instead of
                    ///   dynamic (per instance).
                    /// </summary>
                    public virtual bool IsOptimistic()
                    {
                        return false;
                    }

                    /// <summary>
                    ///   Starts the movement locally, except if this object is
                    ///   optimistic or not owned by the current connection.
                    /// </summary>
                    /// <param name="x">The x position on movement start</param>
                    /// <param name="y">The y position on movement start</param>
                    /// <param name="direction">The movement direction</param>
                    protected override void OnMovementStarted(ushort x, ushort y, Direction direction)
                    {
                        if (IsOptimistic() && IsOwned()) return;
                        base.OnMovementStarted(x, y, direction);
                    }

                    /// <summary>
                    ///   Finished the movement locally, except if this object
                    ///   is optimistic or not owned by the current connection.
                    /// </summary>
                    /// <param name="x">The final x position</param>
                    /// <param name="y">The final y position</param>
                    protected override void OnMovementFinished(ushort x, ushort y)
                    {
                        if (IsOptimistic() && IsOwned()) return;
                        base.OnMovementFinished(x, y);
                    }

                    /// <summary>
                    ///   Reverts the rejected movement.
                    /// </summary>
                    /// <param name="x">The revert x position</param>
                    /// <param name="y">The revert y position</param>
                    protected override void OnMovementRejected(ushort x, ushort y)
                    {
                        if (IsOptimistic() && IsOwned()) MapObject.Teleport(x, y, true);
                    }
                }
            }
        }
    }
}