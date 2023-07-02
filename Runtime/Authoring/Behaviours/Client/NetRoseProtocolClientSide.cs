using AlephVault.Unity.Binary.Wrappers;
using AlephVault.Unity.Meetgard.Authoring.Behaviours.Client;
using AlephVault.Unity.Meetgard.Scopes.Authoring.Behaviours.Client;
using AlephVault.Unity.Meetgard.Types;
using AlephVault.Unity.Support.Utils;
using AlephVault.Unity.NetRose.Types.Models;
using AlephVault.Unity.NetRose.Types.Protocols;
using AlephVault.Unity.NetRose.Types.Protocols.Messages;
using AlephVault.Unity.WindRose.Authoring.Behaviours.World;
using AlephVault.Unity.WindRose.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
                ///   A NetRose protocol is tightly related to a <see cref="ScopesProtocolClientSide"/>.
                ///   In this sense, this protocol sends nothing to the server (save for a local error,
                ///   which is the same case of the scopes protocol), but receives updates from the
                ///   server which reflects, now, in a windrose-aware way.
                /// </summary>
                [RequireComponent(typeof(ScopesProtocolClientSide))]
                public class NetRoseProtocolClientSide : ProtocolClientSide<NetRoseProtocolDefinition>
                {
                    // Whether to debug or not using XDebug.
                    private static bool debug = false;

                    /// <summary>
                    ///   The related <see cref="ScopesProtocolClientSide"/>.
                    /// </summary>
                    public ScopesProtocolClientSide ScopesProtocolClientSide { get; private set; }

                    /// <summary>
                    ///   The current NetRose scope. It is null if the current
                    ///   scope object is null, or it is not a NetRoseScopeClientSide
                    ///   (attached) object.
                    /// </summary>
                    public NetRoseScopeClientSide CurrentNetRoseScope { get; private set; }

                    /// <summary>
                    ///   The current map set (i.e. the current WindRose scope).
                    ///   It is null if the current scope is null, or it is not
                    ///   a WindRose's Scope (attached) object.
                    /// </summary>
                    public Scope CurrentMaps { get; private set; }

                    /// <summary>
                    ///   The lag tolerance for the object. It is the maximum
                    ///   number of delayed steps in the objects queue. If more
                    ///   than this number of steps/movement in the per-object
                    ///   queue is queued, then the queue is accelerated to run
                    ///   all of the steps (ultimately causing a bit of bad
                    ///   experience but recovering from the lag).
                    /// </summary>
                    private ushort lagTolerance = 5;

                    /// <summary>
                    ///   See <see cref="lagTolerance"/>.
                    /// </summary>
                    public ushort LagTolerance => lagTolerance;

                    /// <summary>
                    ///   An after-awake setup.
                    /// </summary>
                    protected override void Setup()
                    {
                        lagTolerance = Math.Max(lagTolerance, (ushort)5);
                        ScopesProtocolClientSide = GetComponent<ScopesProtocolClientSide>();
                        ScopesProtocolClientSide.OnMovedToScope += OnMovedToScope;
                    }

                    private void OnDestroy()
                    {
                        ScopesProtocolClientSide.OnMovedToScope -= OnMovedToScope;
                    }

                    private void OnMovedToScope(ScopeClientSide obj)
                    {
                        XDebug debugger = new XDebug("NetRose", this, $"OnMovedToScope({ScopesProtocolClientSide.CurrentScope?.Id})", debug);
                        debugger.Start();
                        CurrentNetRoseScope = ScopesProtocolClientSide.CurrentScope == null ? null : ScopesProtocolClientSide.CurrentScope.GetComponent<NetRoseScopeClientSide>();
                        debugger.Info((CurrentNetRoseScope == null) ? "Moved to a non-NetRose scope" : "Moved to a NetRose scope");
                        debugger.End();
                    }

                    protected override void SetIncomingMessageHandlers()
                    {
                        AddIncomingMessageHandler<ObjectMessage<Attachment>>("Object:Attached", (proto, message) => {
                            return RunInMainThreadValidatingScopeAndObject(
                                message.ScopeId, message.ObjectId, async (obj) => {
                                    XDebug debugger = new XDebug("NetRose", this, $"HandleMessage:Object:OnAttached({message})", debug);
                                    debugger.Start();
                                    Map map;
                                    try
                                    {
                                        debugger.Info($"Checking map {message.Content.MapIndex}");
                                        map = CurrentNetRoseScope.Maps[(int)message.Content.MapIndex];
                                    }
                                    catch (KeyNotFoundException)
                                    {
                                        debugger.Info($"Index is not known into scope {ScopesProtocolClientSide.CurrentScope?.Id}");
                                        await ScopesProtocolClientSide.LocalError("UnknownMap");
                                        return;
                                    }

                                    ushort x = message.Content.Position.X;
                                    ushort y = message.Content.Position.Y;

                                    debugger.Info($"Checking coordinates ({x}, {y})");
                                    if (!await CheckIsValidMapPosition(map, x, y)) return;

                                    debugger.Info($"Attaching the object at ({x}, {y})");
                                    obj.OnAttached(map, x, y);
                                    debugger.End();
                                }
                            );
                        });
                        AddIncomingMessageHandler<ObjectMessage<Nothing>>("Object:Detached", (proto, message) => {
                            return RunInMainThreadValidatingScopeAndObject(
                                message.ScopeId, message.ObjectId, async (obj) => {
                                    XDebug debugger = new XDebug("NetRose", this, $"HandleMessage:Object:OnDetached({message})", debug);
                                    debugger.Start();
                                    debugger.Info($"Detaching the object");
                                    obj.OnDetached();
                                    debugger.End();
                                }
                            );
                        });
                        AddIncomingMessageHandler<ObjectMessage<MovementStart>>("Object:Movement:Started", (proto, message) => {
                            return RunInMainThreadValidatingScopeAndObject(
                                message.ScopeId, message.ObjectId, async (obj) => {
                                    XDebug debugger = new XDebug("NetRose", this, $"HandleMessage:Object:Movement:Started({message})", debug);
                                    debugger.Start();
                                    ushort x = message.Content.Position.X;
                                    ushort y = message.Content.Position.Y;

                                    debugger.Info($"Checking coordinates ({x}, {y})");
                                    if (!await CheckInValidMapPosition(obj, x, y)) return;

                                    debugger.Info($"Starting movement at ({x}, {y}) with direction: {message.Content.Direction}");
                                    obj.OnMovementStarted(x, y, message.Content.Direction);
                                    debugger.End();
                                }
                            );
                        });
                        AddIncomingMessageHandler<ObjectMessage<Position>>("Object:Movement:Cancelled", (proto, message) => {
                            return RunInMainThreadValidatingScopeAndObject(
                                message.ScopeId, message.ObjectId, async (obj) => {
                                    XDebug debugger = new XDebug("NetRose", this, $"HandleMessage:Object:Movement:Cancelled({message})", debug);
                                    debugger.Start();
                                    ushort x = message.Content.X;
                                    ushort y = message.Content.Y;

                                    debugger.Info($"Checking coordinates ({x}, {y})");
                                    if (!await CheckInValidMapPosition(obj, x, y)) return;

                                    debugger.Info($"Cancelling movement at ({x}, {y})");
                                    obj.OnMovementCancelled(x, y);
                                    debugger.End();
                                }
                            );
                        });
                        AddIncomingMessageHandler<ObjectMessage<Position>>("Object:Movement:Rejected", (proto, message) => {
                            return RunInMainThreadValidatingScopeAndObject(
                                message.ScopeId, message.ObjectId, async (obj) => {
                                    XDebug debugger = new XDebug("NetRose", this, $"HandleMessage:Object:Movement:Rejected({message})", debug);
                                    debugger.Start();
                                    ushort x = message.Content.X;
                                    ushort y = message.Content.Y;

                                    debugger.Info($"Checking coordinates ({x}, {y})");
                                    if (!await CheckInValidMapPosition(obj, x, y)) return;

                                    debugger.Info($"Cancelling movement at ({x}, {y})");
                                    obj.OnMovementRejected(x, y);
                                    debugger.End();
                                }
                            );
                        });
                        AddIncomingMessageHandler<ObjectMessage<Position>>("Object:Movement:Finished", (proto, message) => {
                            return RunInMainThreadValidatingScopeAndObject(
                                message.ScopeId, message.ObjectId, async (obj) => {
                                    XDebug debugger = new XDebug("NetRose", this, $"HandleMessage:Object:Movement:Finished({message})", debug);
                                    debugger.Start();
                                    ushort x = message.Content.X;
                                    ushort y = message.Content.Y;

                                    debugger.Info($"Checking coordinates ({x}, {y})");
                                    if (!await CheckInValidMapPosition(obj, x, y)) return;

                                    debugger.Info($"Finishing movement at ({x}, {y})");
                                    obj.OnMovementFinished(x, y);
                                    debugger.End();
                                }
                            );
                        });
                        AddIncomingMessageHandler<ObjectMessage<Position>>("Object:Teleported", (proto, message) => {
                            return RunInMainThreadValidatingScopeAndObject(
                                message.ScopeId, message.ObjectId, async (obj) => {
                                    XDebug debugger = new XDebug("NetRose", this, $"HandleMessage:Object:Teleported({message})", debug);
                                    debugger.Start();
                                    ushort x = message.Content.X;
                                    ushort y = message.Content.Y;

                                    debugger.Info($"Checking coordinates ({x}, {y})");
                                    if (!await CheckInValidMapPosition(obj, x, y)) return;

                                    debugger.Info($"Teleporting to ({x}, {y})");
                                    obj.OnTeleported(x, y);
                                    debugger.End();
                                }
                            );
                        });
                        AddIncomingMessageHandler<ObjectMessage<UInt>>("Object:Speed:Changed", (proto, message) => {
                            return RunInMainThreadValidatingScopeAndObject(
                                message.ScopeId, message.ObjectId, async (obj) => obj.OnSpeedChanged(message.Content)
                            );
                        });
                        AddIncomingMessageHandler<ObjectMessage<Enum<Direction>>>("Object:Orientation:Changed", (proto, message) => {
                            return RunInMainThreadValidatingScopeAndObject(
                                message.ScopeId, message.ObjectId, async (obj) => obj.OnOrientationChanged(message.Content)
                            );
                        });
                    }

                    // Queues an action in the main thread that checks the current pair
                    // scopeId / objectId for validity and executes a particular action,
                    // or raises a LocaError if invalid. It also raises a LocalError if
                    // the current scope is not a NetRose scope.
                    private Task RunInMainThreadValidatingScopeAndObject(uint scopeId, uint objectId, Func<INetRoseModelClientSide, Task> callback)
                    {
                        return RunInMainThread(async () =>
                        {
                            XDebug debugger = new XDebug("NetRose", this, $"RunInMainThreadValidatingScopeAndObject({scopeId}, {objectId})", debug);
                            debugger.Start();
                            if (!await ScopesProtocolClientSide.RequireIsCurrentScopeAndHoldsObjects(scopeId))
                            {
                                debugger.End();
                                return;
                            }

                            debugger.Info("Checking if the scope is NetRose");
                            if (CurrentNetRoseScope == null)
                            {
                                debugger.Info("Scope is not NetRose");
                                await ScopesProtocolClientSide.LocalError("ScopeIsNotNetRose");
                                debugger.End();
                                return;
                            }

                            debugger.Info("Checking if the object is known");
                            ObjectClientSide obj = ScopesProtocolClientSide.GetObject(objectId);
                            if (obj == null)
                            {
                                debugger.Info("Object is not known");
                                await ScopesProtocolClientSide.LocalError("UnknownObject");
                                debugger.End();
                                return;
                            }

                            debugger.Info("Checking if the object is NetRose");
                            INetRoseModelClientSide netRoseObj = obj.GetComponent<INetRoseModelClientSide>();
                            if (netRoseObj == null)
                            {
                                debugger.Info("Object is not NetRose");
                                await ScopesProtocolClientSide.LocalError("ObjectIsNotNetRose");
                                debugger.End();
                                return;
                            }

                            await callback(netRoseObj);
                            debugger.End();
                        });
                    }

                    // Checks the position to be valid in the map.
                    private async Task<bool> CheckIsValidMapPosition(Map map, ushort x, ushort y)
                    {
                        XDebug debugger = new XDebug("NetRose", this, $"CheckIsValidMapPosition({map}, {x}, {y})", debug);
                        debugger.Start();
                        debugger.Info("Checking if the coordinates are valid into the map");
                        if (map.Width <= x || map.Height <= y)
                        {
                            debugger.Info("Coordinates are not valid");
                            await ScopesProtocolClientSide.LocalError("ObjectPositionOutOfBounds");
                            debugger.End();
                            return false;
                        }

                        debugger.End();
                        return true;
                    }

                    // Checks the object to be in a valid map position.
                    private async Task<bool> CheckInValidMapPosition(INetRoseModelClientSide obj, ushort x, ushort y)
                    {
                        XDebug debugger = new XDebug("NetRose", this, $"CheckInValidMapPosition({obj}, {x}, {y})", debug);
                        debugger.Start();
                        debugger.Info("Checking if the object is attached to a map");
                        Map map = obj.MapObject.ParentMap;
                        if (map == null)
                        {
                            debugger.Info("The object is not attached to a map");
                            await ScopesProtocolClientSide.LocalError("ObjectNotInMap");
                            debugger.End();
                            return false;
                        }

                        debugger.Info("Checking if the coordinates are valid into the object's map");
                        bool result = await CheckIsValidMapPosition(map, x, y);
                        debugger.End();
                        return result;
                    }
                }
            }
        }
    }
}