using AlephVault.Unity.Binary;
using AlephVault.Unity.Meetgard.Scopes.Authoring.Behaviours.Client;
using AlephVault.Unity.Support.Utils;
using AlephVault.Unity.NetRose.Types.Models;
using AlephVault.Unity.WindRose.Authoring.Behaviours.Entities.Objects;
using AlephVault.Unity.WindRose.Authoring.Behaviours.World;
using AlephVault.Unity.WindRose.Types;
using UnityEngine;


namespace AlephVault.Unity.NetRose
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace Client
            {
                /// <summary>
                ///   Map objects exist in client side, reflecting what is received
                ///   from the server side, and applying some internal mechanisms
                ///   for lag balance and recovery. These ones are also related to
                ///   a single WindRose map object in a single map.
                /// </summary>
                [RequireComponent(typeof(MapObject))]
                public abstract partial class NetRoseModelClientSide<SpawnData, RefreshData> : ModelClientSide<MapObjectModel<SpawnData>, MapObjectModel<RefreshData>>, INetRoseModelClientSide
                    where SpawnData : class, ISerializable, new()
                    where RefreshData : class, ISerializable, new()
                {
                    // Whether to debug or not using XDebug.
                    private static bool debug = false;

                    /// <summary>
                    ///   The related WindRose map object.
                    /// </summary>
                    public MapObject MapObject { get; private set; }

                    // The lag tolerance, as retrieved from the protocol.
                    private ushort lagTolerance;

                    // Tells whether it is spawned or not.
                    private bool spawned = false;

                    protected virtual void Awake()
                    {
                        OnSpawned += NetRoseModelClientSide_OnSpawned;
                        OnDespawned += NetRoseModelClientSide_OnDespawned;
                        MapObject = GetComponent<MapObject>();
                        MapObject.onMovementFinished.AddListener(OnMovementFinished);
                        MapObject.onMovementCancelled.AddListener(OnMovementCancelled);
                    }

                    protected virtual void OnDestroy()
                    {
                        OnSpawned -= NetRoseModelClientSide_OnSpawned;
                        OnDespawned -= NetRoseModelClientSide_OnDespawned;
                        MapObject.onMovementFinished.RemoveListener(OnMovementFinished);
                        MapObject.onMovementCancelled.RemoveListener(OnMovementCancelled);
                    }

                    // On spawn, set the lag tolerance, and the spawned.
                    private void NetRoseModelClientSide_OnSpawned()
                    {
                        NetRoseProtocolClientSide protocol = Protocol.GetComponent<NetRoseProtocolClientSide>();
                        lagTolerance = protocol != null ? protocol.LagTolerance : (ushort)5;
                        spawned = true;
                    }

                    // On despawn, clear the lag tolerance, the spawned flag, and the queue.
                    private void NetRoseModelClientSide_OnDespawned()
                    {
                        lagTolerance = 0;
                        spawned = false;
                        queue.Clear();
                    }

                    private void OnMovementFinished(Direction movement)
                    {
                        if (spawned && !runningQueue) RunQueue(false);
                    }

                    private void OnMovementCancelled(Direction? movement)
                    {
                        if (spawned && !runningQueue) RunQueue(false);
                    }

                    //
                    // From this point, all the network-related events start.
                    //

                    /// <summary>
                    ///   Clears the command queue to perform an immediate
                    ///   action that comes next.
                    /// </summary>
                    protected void Immediate()
                    {
                        queue.Clear();
                    }

                    // Processes an attached event.
                    void INetRoseModelClientSide.OnAttached(Map map, ushort x, ushort y)
                    {
                        if (!spawned) return;
                        OnAttached(map, x, y);
                    }

                    /// <summary>
                    ///   The true implementation of an attach message
                    ///   is to clear everything and do it immediately.
                    /// </summary>
                    /// <param name="map">The new map</param>
                    /// <param name="x">The target x position</param>
                    /// <param name="y">The target y position</param>
                    protected virtual void OnAttached(Map map, ushort x, ushort y)
                    {
                        MapObject.Attach(map, x, y, true);
                    }

                    // Processes a detached event.
                    void INetRoseModelClientSide.OnDetached()
                    {
                        if (!spawned) return;
                        OnDetached();
                    }

                    /// <summary>
                    ///   The true implementation of a detach message
                    ///   is to clear everything and do it immediately.
                    /// </summary>
                    /// <param name="x">The target x position</param>
                    /// <param name="y">The target y position</param>
                    protected virtual void OnDetached()
                    {
                        Immediate();
                        MapObject.Detach();
                    }

                    // Processes a teleport event.
                    void INetRoseModelClientSide.OnTeleported(ushort x, ushort y)
                    {
                        if (!spawned) return;
                        OnTeleported(x, y);
                    }

                    /// <summary>
                    ///   The true implementation of a teleport message
                    ///   is to clear everything and do it immediately.
                    /// </summary>
                    /// <param name="x">The target x position</param>
                    /// <param name="y">The target y position</param>
                    protected virtual void OnTeleported(ushort x, ushort y)
                    {
                        Immediate();
                        MapObject.Teleport(x, y);
                    }

                    // Processes a movement start event.
                    void INetRoseModelClientSide.OnMovementStarted(ushort x, ushort y, Direction direction)
                    {
                        if (!spawned) return;
                        OnMovementStarted(x, y, direction);
                    }

                    /// <summary>
                    ///   The true implementation of a movement started
                    ///   message is to queue a start movement command.
                    /// </summary>
                    /// <param name="x">The x position on movement start</param>
                    /// <param name="y">The y position on movement start</param>
                    /// <param name="direction">The movement direction</param>
                    protected virtual void OnMovementStarted(ushort x, ushort y, Direction direction)
                    {
                        QueueElement(new MovementStartCommand() { StartX = x, StartY = y, Direction = direction });
                    }

                    // Processes a movement cancel event.
                    void INetRoseModelClientSide.OnMovementCancelled(ushort x, ushort y)
                    {
                        if (!spawned) return;
                        OnMovementCancelled(x, y);
                    }

                    /// <summary>
                    ///   The true implementation of a cancel movement
                    ///   message is to queue the cancel movement command.
                    /// </summary>
                    /// <param name="x">The x position to revert to</param>
                    /// <param name="y">The y position to revert to</param>
                    protected virtual void OnMovementCancelled(ushort x, ushort y)
                    {
                        QueueElement(new MovementCancelCommand() { RevertX = x, RevertY = y });
                    }

                    // Processes a movement rejection event.
                    void INetRoseModelClientSide.OnMovementRejected(ushort x, ushort y)
                    {
                        if (!spawned) return;
                        OnMovementRejected(x, y);
                    }

                    /// <summary>
                    ///   The true implementation of a reject movement
                    ///   event is empty. It will be used later.
                    /// </summary>
                    /// <param name="x">The x position to revert to</param>
                    /// <param name="y">The y position to revert to</param>
                    protected virtual void OnMovementRejected(ushort x, ushort y)
                    {
                    }

                    // Processes a movement finish event.
                    void INetRoseModelClientSide.OnMovementFinished(ushort x, ushort y)
                    {
                        if (!spawned) return;
                        OnMovementFinished(x, y);
                    }

                    /// <summary>
                    ///   The true implementation of a finish movement message
                    ///   is to queue the finish movement command.
                    /// </summary>
                    /// <param name="x">The final x position</param>
                    /// <param name="y">The final y position</param>
                    protected virtual void OnMovementFinished(ushort x, ushort y)
                    {
                        QueueElement(new MovementFinishCommand() { EndX = x, EndY = y });
                    }

                    // Processes a movement speed change event.
                    void INetRoseModelClientSide.OnSpeedChanged(uint speed)
                    {
                        if (!spawned) return;
                        QueueElement(new SpeedChangeCommand() { Speed = speed });
                    }

                    // Processes an orientation change event.
                    void INetRoseModelClientSide.OnOrientationChanged(Direction orientation)
                    {
                        if (!spawned) return;
                        QueueElement(new OrientationChangeCommand() { Orientation = orientation });
                    }

                    // Performs the base inflation (i.e. refreshing base MapObject data).
                    private void InflateBase(Status status, Direction orientation, uint speed, bool clearQueue)
                    {
                        MapObject.Orientation = orientation;
                        MapObject.Speed = speed;
                        if (status != null)
                        {
                            Attachment attachment = status.Attachment;
                            Map map = Scope.GetComponent<Scope>()[(int)attachment.MapIndex];
                            if (clearQueue) queue.Clear();
                            MapObject.Attach(map, attachment.Position.X, attachment.Position.Y, true);
                            Direction? movement = status.Movement;
                            if (movement != null)
                            {
                                QueueElement(new MovementStartCommand()
                                {
                                    StartX = attachment.Position.X,
                                    StartY = attachment.Position.Y,
                                    Direction = movement.Value
                                });
                            }
                        }
                    }

                    /// <summary>
                    ///   Updates the object with the full spawn data (attachment,
                    ///   movement and model).
                    /// </summary>
                    /// <param name="fullData">The wrapped full model data to inflate from</param>
                    protected override void InflateFrom(MapObjectModel<SpawnData> fullData)
                    {
                        // Also initializing the object here, to avoid executing
                        // that behaviour in Start(). That one caused the movement
                        // to be cancelled, if the object was to be spawned for
                        // the first time and immediately (after Awake()) a new
                        // movement was issued: Initialize() would cause another
                        // forced attachment, which would ultimately cancel the
                        // queued/issued new movement.
                        XDebug debugger = new XDebug("NetRose", this, $"InflateFrom({fullData})", debug);
                        debugger.Start();
                        debugger.Info("Initializing the object");
                        MapObject.Initialize();
                        debugger.Info("Inflating the object's status");
                        InflateBase(fullData.Status, fullData.Orientation, fullData.Speed, false);
                        debugger.Info("Inflating the object's model");
                        InflateFrom(fullData.Data);
                        debugger.End();
                    }

                    /// <summary>
                    ///   Inflates the object with the full model data, after processing
                    ///   wrapping attachment and movement.
                    /// </summary>
                    /// <param name="fullData">The full model data to inflate from</param>
                    protected abstract void InflateFrom(SpawnData fullData);

                    /// <summary>
                    ///   Updates the object with the full model data, which also involves
                    ///   base-inflating part of the data.
                    /// </summary>
                    /// <param name="refreshData">The wrapped refresh model data to update from</param>
                    protected override void UpdateFrom(MapObjectModel<RefreshData> refreshData)
                    {
                        XDebug debugger = new XDebug("NetRose", this, $"UpdateFrom({refreshData})", debug);
                        debugger.Start();
                        debugger.Info("Inflating the object's status");
                        InflateBase(refreshData.Status, refreshData.Orientation, refreshData.Speed, true);
                        debugger.Info("Inflating the object's model");
                        UpdateFrom(refreshData.Data);
                        debugger.End();
                    }

                    /// <summary>
                    ///   Updates the object with the refresh model data, after processing
                    ///   wrapping attachment and movement.
                    /// </summary>
                    /// <param name="refreshData">The refresh model data to update from</param>
                    protected abstract void UpdateFrom(RefreshData refreshData);
                }
            }
        }
    }
}
