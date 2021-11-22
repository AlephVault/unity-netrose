using AlephVault.Unity.Binary;
using AlephVault.Unity.Meetgard.Scopes.Authoring.Behaviours.Server;
using GameMeanMachine.Unity.WindRose.Authoring.Behaviours.Entities.Objects;
using GameMeanMachine.Unity.WindRose.Authoring.Behaviours.World;
using GameMeanMachine.Unity.WindRose.Types;
using GameMeanMachine.Unity.NetRose.Types.Models;
using System;
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
                ///   Map objects exist in server side, reflecting what is changed
                ///   in them to the client side, and sending those changes to the
                ///   client side by means of the current scope. These ones are also
                ///   related to a single WindRose map object in a single map.
                /// </summary>
                [RequireComponent(typeof(MapObject))]
                public abstract class NetRoseModelServerSide<SpawnData, RefreshData> : ModelServerSide<MapObjectModel<SpawnData>, MapObjectModel<RefreshData>>, INetRoseModelServerSide
                    where SpawnData : class, ISerializable, new()
                    where RefreshData : class, ISerializable, new()
                {
                    /// <summary>
                    ///   The related WindRose MapObject.
                    /// </summary>
                    public MapObject MapObject { get; private set; }

                    /// <summary>
                    ///   The NetRose scope server side this object belongs to.
                    ///   It MAY be null if the scope is not a NetRose one! Be careful with this.
                    /// </summary>
                    public NetRoseScopeServerSide NetRoseScopeServerSide { get; private set; }

                    protected void Awake()
                    {
                        MapObject = GetComponent<MapObject>();
                        OnSpawned += ObjectServerSide_OnSpawned;
                        OnDespawned += ObjectServerSide_OnDespawned;
                    }

                    protected void Start()
                    {
                        MapObject.onAttached.AddListener(OnAttached);
                        MapObject.onDetached.AddListener(OnDetached);
                        MapObject.onMovementStarted.AddListener(OnMovementStarted);
                        MapObject.onMovementFinished.AddListener(OnMovementFinished);
                        MapObject.onMovementCancelled.AddListener(OnMovementCancelled);
                        MapObject.onTeleported.AddListener(OnTeleported);
                        MapObject.onOrientationChanged.AddListener(OnOrientationChanged);
                        MapObject.onSpeedChanged.AddListener(OnSpeedChanged);
                        base.Start();
                    }

                    protected void OnDestroy()
                    {
                        OnSpawned -= ObjectServerSide_OnSpawned;
                        OnDespawned -= ObjectServerSide_OnDespawned;
                        MapObject.onAttached.RemoveListener(OnAttached);
                        MapObject.onDetached.RemoveListener(OnDetached);
                        MapObject.onMovementStarted.RemoveListener(OnMovementStarted);
                        MapObject.onMovementFinished.RemoveListener(OnMovementFinished);
                        MapObject.onMovementCancelled.RemoveListener(OnMovementCancelled);
                        MapObject.onTeleported.RemoveListener(OnTeleported);
                        MapObject.onOrientationChanged.RemoveListener(OnOrientationChanged);
                        MapObject.onSpeedChanged.RemoveListener(OnSpeedChanged);
                    }

                    private async Task ObjectServerSide_OnSpawned()
                    {
                        NetRoseScopeServerSide = Scope.GetComponent<NetRoseScopeServerSide>();
                        currentStatus = GetCurrentStatus();
                    }

                    private async Task ObjectServerSide_OnDespawned()
                    {
                        NetRoseScopeServerSide = null;
                    }

                    private void RunInMainThreadIfSpawned(Action callback)
                    {
                        Protocol.RunInMainThread(() =>
                        {
                            if (NetRoseScopeServerSide != null) callback();
                        });
                    }

                    private void OnAttached(Map map)
                    {
                        Status newStatus = GetCurrentStatus();
                        ushort x = MapObject.X;
                        ushort y = MapObject.Y;
                        int mapIdx = map.GetIndex();
                        RunInMainThreadIfSpawned(() =>
                        {
                            currentStatus = newStatus;
                            // Please note: By this point, we're in the appropriate scope.
                            // This means that the given map belongs to the current scope.
                            _ = NetRoseScopeServerSide.BroadcastObjectAttached(
                                Id, (uint)mapIdx, x, y
                            );
                        });
                    }

                    private void OnDetached()
                    {
                        Status newStatus = GetCurrentStatus();
                        RunInMainThreadIfSpawned(() =>
                        {
                            currentStatus = newStatus;
                            _ = NetRoseScopeServerSide.BroadcastObjectDetached(Id);
                        });
                    }

                    private void OnMovementStarted(Direction direction)
                    {
                        Status newStatus = GetCurrentStatus();
                        ushort x = MapObject.X;
                        ushort y = MapObject.Y;
                        RunInMainThreadIfSpawned(() =>
                        {
                            currentStatus = newStatus;
                            _ = NetRoseScopeServerSide.BroadcastObjectMovementStarted(
                                Id, x, y, direction
                            );
                        });
                    }

                    private void OnMovementFinished(Direction direction)
                    {
                        Status newStatus = GetCurrentStatus();
                        ushort x = MapObject.X;
                        ushort y = MapObject.Y;
                        RunInMainThreadIfSpawned(() =>
                        {
                            currentStatus = newStatus;
                            _ = NetRoseScopeServerSide.BroadcastObjectMovementFinished(
                                Id, x, y
                            );
                        });
                    }

                    private void OnMovementCancelled(Direction? direction)
                    {
                        Status newStatus = GetCurrentStatus();
                        ushort x = MapObject.X;
                        ushort y = MapObject.Y;
                        RunInMainThreadIfSpawned(() =>
                        {
                            currentStatus = newStatus;
                            _ = NetRoseScopeServerSide.BroadcastObjectMovementCancelled(
                                Id, x, y
                            );
                        });
                    }

                    private void OnTeleported(ushort x, ushort y)
                    {
                        Status newStatus = GetCurrentStatus();
                        RunInMainThreadIfSpawned(() =>
                        {
                            currentStatus = newStatus;
                            _ = NetRoseScopeServerSide.BroadcastObjectTeleported(Id, x, y);
                        });
                    }

                    private void OnOrientationChanged(Direction direction)
                    {
                        RunInMainThreadIfSpawned(() =>
                        {
                            _ = NetRoseScopeServerSide.BroadcastObjectOrientationChanged(Id, direction);
                        });
                    }

                    private void OnSpeedChanged(uint speed)
                    {
                        RunInMainThreadIfSpawned(() =>
                        {
                            _ = NetRoseScopeServerSide.BroadcastObjectSpeedChanged(Id, speed);
                        });
                    }

                    private Status currentStatus = null;

                    private Status GetCurrentStatus()
                    {
                        if (MapObject.ParentMap != null)
                        {
                            int index = MapObject.ParentMap.GetIndex();
                            // If index is -1, the map is not synchronized.
                            // In this case, the current status must be null
                            // and nothing must be synchronized actually, for
                            // no scope is currently the object in.
                            return (index == -1) ? null : new Status()
                            {
                                Attachment = new Attachment()
                                {
                                    Position = new Position()
                                    {
                                        X = MapObject.X,
                                        Y = MapObject.Y
                                    },
                                    MapIndex = (uint)index
                                },
                                Movement = MapObject.Movement
                            };
                        }
                        else
                        {
                            return null;
                        }
                    }

                    protected override MapObjectModel<SpawnData> GetFullData(ulong connectionId)
                    {
                        return new MapObjectModel<SpawnData>() {
                            Status = currentStatus,
                            Orientation = MapObject.Orientation,
                            Speed = MapObject.Speed,
                            Data = GetInnerFullData(connectionId)
                        };
                    }

                    protected abstract SpawnData GetInnerFullData(ulong connectionId);

                    protected override MapObjectModel<RefreshData> GetRefreshData(ulong connectionId, string context)
                    {
                        return new MapObjectModel<RefreshData>() {
                            Status = currentStatus,
                            Orientation = MapObject.Orientation,
                            Speed = MapObject.Speed,
                            Data = GetInnerRefreshData(connectionId, context)
                        };
                    }

                    protected abstract RefreshData GetInnerRefreshData(ulong connectionId, string context);
                }
            }
        }
    }
}
