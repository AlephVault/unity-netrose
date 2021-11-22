﻿using AlephVault.Unity.Binary.Wrappers;
using AlephVault.Unity.Meetgard.Authoring.Behaviours.Client;
using AlephVault.Unity.Meetgard.Authoring.Behaviours.Server;
using AlephVault.Unity.Meetgard.Scopes.Authoring.Behaviours.Client;
using AlephVault.Unity.Meetgard.Scopes.Authoring.Behaviours.Server;
using AlephVault.Unity.Meetgard.Types;
using AlephVault.Unity.Support.Utils;
using GameMeanMachine.Unity.NetRose.Types.Models;
using GameMeanMachine.Unity.NetRose.Types.Protocols;
using GameMeanMachine.Unity.NetRose.Types.Protocols.Messages;
using GameMeanMachine.Unity.WindRose.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GameMeanMachine.Unity.NetRose
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace Server
            {
                /// <summary>
                ///   A NetRose protocol is tightly related to a <see cref="ScopesProtocolServerSide"/>.
                ///   In this sense, this protocol gets nothing to the client (save for a local error,
                ///   which is actually handled by the scopes protocol), but sends updates from the
                ///   client which reflects, now, in a windrose-aware way.
                /// </summary>
                [RequireComponent(typeof(ScopesProtocolServerSide))]
                public class NetRoseProtocolServerSide : ProtocolServerSide<NetRoseProtocolDefinition>
                {
                    // Whether to debug or not using XDebug.
                    private static bool debug = false;

                    /// <summary>
                    ///   The related <see cref="ScopesProtocolServerSide"/>.
                    /// </summary>
                    public ScopesProtocolServerSide ScopesProtocolServerSide { get; private set; }

                    private Func<IEnumerable<ulong>, ObjectMessage<Attachment>, Dictionary<ulong, Task>> ObjectAttachedBroadcaster;
                    private Func<IEnumerable<ulong>, ObjectMessage<Nothing>, Dictionary<ulong, Task>> ObjectDetachedBroadcaster;
                    private Func<IEnumerable<ulong>, ObjectMessage<MovementStart>, Dictionary<ulong, Task>> ObjectMovementStartedBroadcaster;
                    private Func<IEnumerable<ulong>, ObjectMessage<Position>, Dictionary<ulong, Task>> ObjectMovementCancelledBroadcaster;
                    private Func<IEnumerable<ulong>, ObjectMessage<Position>, Dictionary<ulong, Task>> ObjectMovementFinishedBroadcaster;
                    private Func<IEnumerable<ulong>, ObjectMessage<Position>, Dictionary<ulong, Task>> ObjectTeleportedBroadcaster;
                    private Func<IEnumerable<ulong>, ObjectMessage<UInt>, Dictionary<ulong, Task>> ObjectSpeedChangedBroadcaster;
                    private Func<IEnumerable<ulong>, ObjectMessage<Enum<Direction>>, Dictionary<ulong, Task>> ObjectOrientationChangedBroadcaster;

                    /// <summary>
                    ///   An after-awake setup.
                    /// </summary>
                    protected override void Setup()
                    {
                        ScopesProtocolServerSide = GetComponent<ScopesProtocolServerSide>();
                    }

                    protected override void Initialize()
                    {
                        ObjectAttachedBroadcaster = MakeBroadcaster<ObjectMessage<Attachment>>("Object:Attached");
                        ObjectDetachedBroadcaster = MakeBroadcaster<ObjectMessage<Nothing>>("Object:Detached");
                        ObjectMovementStartedBroadcaster = MakeBroadcaster<ObjectMessage<MovementStart>>("Object:Movement:Started");
                        ObjectMovementCancelledBroadcaster = MakeBroadcaster<ObjectMessage<Position>>("Object:Movement:Cancelled");
                        ObjectMovementFinishedBroadcaster = MakeBroadcaster<ObjectMessage<Position>>("Object:Movement:Finished");
                        ObjectTeleportedBroadcaster = MakeBroadcaster<ObjectMessage<Position>>("Object:Teleported");
                        ObjectSpeedChangedBroadcaster = MakeBroadcaster<ObjectMessage<UInt>>("Object:Speed:Changed");
                        ObjectOrientationChangedBroadcaster = MakeBroadcaster<ObjectMessage<Enum<Direction>>>("Object:Orientation:Changed");
                    }

                    // From now, a lot of internal functions are to be dispatched.

                    /// <summary>
                    ///   Broadcasts a "object attached" message. This message is triggered when
                    ///   an object is added to a map in certain scope.
                    /// </summary>
                    /// <param name="connections">The connections to send this message to</param>
                    /// <param name="scopeId">The id of the scope the object belongs to</param>
                    /// <param name="objectId">The id of the object</param>
                    /// <param name="mapIndex">The index of the map, inside the scope, this object is being added to</param>
                    /// <param name="x">The x position of the object in the new map</param>
                    /// <param name="y">The y position of the object in the new map</param>
                    internal Task BroadcastObjectAttached(IEnumerable<ulong> connections, uint scopeId, uint objectId, uint mapIndex, ushort x, ushort y)
                    {
                        return UntilBroadcastIsDone(ObjectAttachedBroadcaster(connections, new ObjectMessage<Attachment>()
                        {
                            ScopeId = scopeId,
                            ObjectId = objectId,
                            Content = new Attachment() { MapIndex = mapIndex, Position = new Position() { X = x, Y = y } }
                        }));
                    }

                    /// <summary>
                    ///   Broadcasts a "object attached" message. This message is triggered when
                    ///   an object is added to a map in certain scope.
                    /// </summary>
                    /// <param name="connections">The connections to send this message to</param>
                    /// <param name="scopeId">The id of the scope the object belongs to</param>
                    /// <param name="objectId">The id of the object</param>
                    internal Task BroadcastObjectDetached(IEnumerable<ulong> connections, uint scopeId, uint objectId)
                    {
                        return UntilBroadcastIsDone(ObjectDetachedBroadcaster(connections, new ObjectMessage<Nothing>()
                        {
                            ScopeId = scopeId,
                            ObjectId = objectId,
                            Content = new Nothing()
                        }));
                    }

                    /// <summary>
                    ///   Broadcasts a "object movement started" message. This message is triggered when
                    ///   an object started moving inside a map in certain scope.
                    /// </summary>
                    /// <param name="connections">The connections to send this message to</param>
                    /// <param name="scopeId">The id of the scope the object belongs to</param>
                    /// <param name="objectId">The id of the object</param>
                    /// <param name="x">The starting x position of the object when starting movement</param>
                    /// <param name="y">The starting y position of the object when starting movement</param>
                    /// <param name="direction">The direction of the object when starting movement</param>
                    internal Task BroadcastObjectMovementStarted(IEnumerable<ulong> connections, uint scopeId, uint objectId, ushort x, ushort y, Direction direction)
                    {
                        return UntilBroadcastIsDone(ObjectMovementStartedBroadcaster(connections, new ObjectMessage<MovementStart>()
                        {
                            ScopeId = scopeId,
                            ObjectId = objectId,
                            Content = new MovementStart() { Position = new Position() { X = x, Y = y }, Direction = direction }
                        }));
                    }

                    /// <summary>
                    ///   Broadcasts a "object movement cancelled" message. This message is triggered when
                    ///   an object cancelled moving inside a map in certain scope.
                    /// </summary>
                    /// <param name="connections">The connections to send this message to</param>
                    /// <param name="scopeId">The id of the scope the object belongs to</param>
                    /// <param name="objectId">The id of the object</param>
                    /// <param name="x">The to-revert x position of the object when cancelling movement</param>
                    /// <param name="y">The to-revert y position of the object when cancelling movement</param>
                    internal Task BroadcastObjectMovementCancelled(IEnumerable<ulong> connections, uint scopeId, uint objectId, ushort x, ushort y)
                    {
                        return UntilBroadcastIsDone(ObjectMovementCancelledBroadcaster(connections, new ObjectMessage<Position>()
                        {
                            ScopeId = scopeId,
                            ObjectId = objectId,
                            Content = new Position() { X = x, Y = y }
                        }));
                    }

                    /// <summary>
                    ///   Broadcasts a "object movement finished" message. This message is triggered when
                    ///   an object finished moving inside a map in certain scope.
                    /// </summary>
                    /// <param name="connections">The connections to send this message to</param>
                    /// <param name="scopeId">The id of the scope the object belongs to</param>
                    /// <param name="objectId">The id of the object</param>
                    /// <param name="x">The end x position of the object when finishing movement</param>
                    /// <param name="y">The end y position of the object when finishing movement</param>
                    internal Task BroadcastObjectMovementFinished(IEnumerable<ulong> connections, uint scopeId, uint objectId, ushort x, ushort y)
                    {
                        return UntilBroadcastIsDone(ObjectMovementFinishedBroadcaster(connections, new ObjectMessage<Position>()
                        {
                            ScopeId = scopeId,
                            ObjectId = objectId,
                            Content = new Position() { X = x, Y = y }
                        }));
                    }

                    /// <summary>
                    ///   Broadcasts a "object movement teleported" message. This message is triggered when
                    ///   an object teleported inside a map in certain scope.
                    /// </summary>
                    /// <param name="connections">The connections to send this message to</param>
                    /// <param name="scopeId">The id of the scope the object belongs to</param>
                    /// <param name="objectId">The id of the object</param>
                    /// <param name="x">The end x position of the object when teleporting</param>
                    /// <param name="y">The end y position of the object when teleporting</param>
                    internal Task BroadcastObjectTeleported(IEnumerable<ulong> connections, uint scopeId, uint objectId, ushort x, ushort y)
                    {
                        return UntilBroadcastIsDone(ObjectTeleportedBroadcaster(connections, new ObjectMessage<Position>()
                        {
                            ScopeId = scopeId,
                            ObjectId = objectId,
                            Content = new Position() { X = x, Y = y }
                        }));
                    }

                    /// <summary>
                    ///   Broadcasts a "object speed changed" message. This message is triggered when
                    ///   an object changed its speed inside a map in certain scope.
                    /// </summary>
                    /// <param name="connections">The connections to send this message to</param>
                    /// <param name="scopeId">The id of the scope the object belongs to</param>
                    /// <param name="objectId">The id of the object</param>
                    /// <param name="speed">The new object speed</param>
                    internal Task BroadcastObjectSpeedChanged(IEnumerable<ulong> connections, uint scopeId, uint objectId, uint speed)
                    {
                        return UntilBroadcastIsDone(ObjectSpeedChangedBroadcaster(connections, new ObjectMessage<UInt>()
                        {
                            ScopeId = scopeId,
                            ObjectId = objectId,
                            Content = (UInt)speed
                        }));
                    }

                    /// <summary>
                    ///   Broadcasts a "object orientation changed" message. This message is triggered when
                    ///   an object changed its orientation inside a map in certain scope.
                    /// </summary>
                    /// <param name="connections">The connections to send this message to</param>
                    /// <param name="scopeId">The id of the scope the object belongs to</param>
                    /// <param name="objectId">The id of the object</param>
                    /// <param name="orientation">The new object orientation</param>
                    internal Task BroadcastObjectOrientationChanged(IEnumerable<ulong> connections, uint scopeId, uint objectId, Direction orientation)
                    {
                        return UntilBroadcastIsDone(ObjectOrientationChangedBroadcaster(connections, new ObjectMessage<Enum<Direction>>()
                        {
                            ScopeId = scopeId,
                            ObjectId = objectId,
                            Content = (Enum<Direction>)orientation
                        }));
                    }

                    /// <summary>
                    ///   Initializes a prefab. If it is a netrose server side
                    ///   object, it invokes Initialize() in the related map
                    ///   object before returning it.
                    /// </summary>
                    /// <param name="prefabId">The index of the prefab to instantiate</param>
                    /// <param name="callback">The optional callback to invoke in the new object</param>
                    /// <returns>The instance</returns>
                    public ObjectServerSide InstantiateHere(uint prefabId, Action<ObjectServerSide> callback = null)
                    {
                        XDebug debugger = new XDebug("NetRose", this, $"InstantiateHere({prefabId})", debug);
                        debugger.Start();
                        debugger.Info("Instantiating the object");
                        ObjectServerSide obj = ScopesProtocolServerSide.InstantiateHere(prefabId);
                        debugger.Info("Invoking a custom init callback, if any");
                        callback?.Invoke(obj);
                        debugger.Info("Invoking its Initialize method, if it is a NetRose object");
                        if (obj is INetRoseModelServerSide netroseObj) netroseObj.MapObject.Initialize();
                        debugger.End();
                        return obj;
                    }

                    /// <summary>
                    ///   Initializes a prefab. If it is a netrose server side
                    ///   object, it invokes Initialize() in the related map
                    ///   object before returning it.
                    /// </summary>
                    /// <param name="prefab">The key of the prefab to instantiate</param>
                    /// <param name="callback">The optional callback to invoke in the new object</param>
                    /// <returns>The instance</returns>
                    public ObjectServerSide InstantiateHere(string key, Action<ObjectServerSide> callback = null)
                    {
                        XDebug debugger = new XDebug("NetRose", this, $"InstantiateHere({key})", debug);
                        debugger.Start();
                        debugger.Info("Instantiating the object");
                        ObjectServerSide obj = ScopesProtocolServerSide.InstantiateHere(key);
                        debugger.Info("Invoking a custom init callback, if any");
                        callback?.Invoke(obj);
                        debugger.Info("Invoking its Initialize method, if it is a NetRose object");
                        if (obj is INetRoseModelServerSide netroseObj) netroseObj.MapObject.Initialize();
                        debugger.End();
                        return obj;
                    }

                    /// <summary>
                    ///   Initializes a prefab. If it is a netrose server side
                    ///   object, it invokes Initialize() in the related map
                    ///   object before returning it.
                    /// </summary>
                    /// <param name="prefab">The prefab to instantiate</param>
                    /// <param name="callback">The optional callback to invoke in the new object</param>
                    /// <returns>The instance</returns>
                    public ObjectServerSide InstantiateHere(ObjectServerSide prefab, Action<ObjectServerSide> callback = null)
                    {
                        XDebug debugger = new XDebug("NetRose", this, $"InstantiateHere({prefab})", debug);
                        debugger.Start();
                        debugger.Info("Instantiating the object");
                        ObjectServerSide obj = ScopesProtocolServerSide.InstantiateHere(prefab);
                        debugger.Info("Invoking a custom init callback, if any");
                        callback?.Invoke(obj);
                        debugger.Info("Invoking its Initialize method, if it is a NetRose object");
                        if (obj is INetRoseModelServerSide netroseObj) netroseObj.MapObject.Initialize();
                        debugger.End();
                        return obj;
                    }
                }
            }
        }
    }
}