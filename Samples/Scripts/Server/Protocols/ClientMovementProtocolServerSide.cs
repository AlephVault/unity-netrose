using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using AlephVault.Unity.Meetgard.Authoring.Behaviours.Server;
using AlephVault.Unity.Meetgard.Scopes.Authoring.Behaviours.Server;
using GameMeanMachine.Unity.NetRose.Authoring.Behaviours.Server;
using GameMeanMachine.Unity.WindRose.Authoring.Behaviours.World;
using GameMeanMachine.Unity.WindRose.Authoring.Behaviours.Entities.Objects;
using GameMeanMachine.Unity.WindRose.Types;
using GameMeanMachine.Unity.NetRose.Samples.Common.Protocols;

namespace GameMeanMachine.Unity.NetRose
{
    namespace Samples
    {
        namespace Server
        {
            namespace Protocols
            {
                [RequireComponent(typeof(NetRoseProtocolServerSide))]
                public class ClientMovementProtocolServerSide : ProtocolServerSide<ClientMovementProtocolDefinition>
                {
                    private class ObjectOwnage
                    {
                        public float LastCommandTime = 0;
                        public INetRoseModelServerSide OwnedObject = null;
                    }

                    private Dictionary<ulong, ObjectOwnage> objects = new Dictionary<ulong, ObjectOwnage>();

                    [SerializeField]
                    private int char1index = 1;

                    [SerializeField]
                    private int char2index = 2;

                    private NetRoseProtocolServerSide NetRoseProtocolServerSide;

                    protected override void Setup()
                    {
                        NetRoseProtocolServerSide = GetComponent<NetRoseProtocolServerSide>();
                    }

                    public override async Task OnServerStarted()
                    {
                        objects = new Dictionary<ulong, ObjectOwnage>();
                    }

                    public override async Task OnServerStopped(System.Exception e)
                    {
                        objects = null;
                    }

                    public override async Task OnConnected(ulong clientId)
                    {
                        Debug.Log($"ClientMovementProtocolServerSide.OnConnected({clientId})::Queue");
                        var _ = RunInMainThread(() =>
                        {
                            Debug.Log($"ClientMovementProtocolServerSide.OnConnected({clientId})::Start");
                            // Which index to take the character from?
                            int index = (clientId % 2 == 1) ? char1index : char2index;
                            // Instantiate it.
                            ObjectServerSide obj = NetRoseProtocolServerSide.InstantiateHere((uint)index, (obj) => {
                                Debug.Log($"ClientMovementProtocolServerSide.OnConnected({clientId})::--Picking index: {index}");
                                // Get the netrose component of it.
                                OwnableModelServerSide ownableObj = obj.GetComponent<OwnableModelServerSide>();
                                Debug.Log($"ClientMovementProtocolServerSide.OnConnected({clientId})::--Getting ownable obj", ownableObj);
                                // Give it the required connection id.
                                ownableObj.ConnectionId = clientId;
                                // Add it to the dictionary.
                                Debug.Log($"ClientMovementProtocolServerSide.OnConnected({clientId})::--Registering", ownableObj);
                                objects[clientId] = new ObjectOwnage() { LastCommandTime = 0, OwnedObject = obj.GetComponent<INetRoseModelServerSide>() };
                                // Initialize it in no map.
                            });
                            // Attach it to a map.
                            Debug.Log($"ClientMovementProtocolServerSide.OnConnected({clientId})::--Attaching", obj);
                            obj.GetComponent<INetRoseModelServerSide>().MapObject.Attach(
                                NetRoseProtocolServerSide.ScopesProtocolServerSide.LoadedScopes[4].GetComponent<Scope>()[0],
                                8, 6, true
                            );
                            Debug.Log($"ClientMovementProtocolServerSide.OnConnected({clientId})::End");
                        });
                    }

                    public override async Task OnDisconnected(ulong clientId, System.Exception reason)
                    {
                        var _ = RunInMainThread(() =>
                        {
                            Debug.Log($"ClientMovementProtocolServerSide.OnDisconnected({clientId})::Start");
                            if (objects.TryGetValue(clientId, out ObjectOwnage ownage))
                            {
                                // It will de-spawn and destroy the object.
                                Destroy(ownage.OwnedObject.MapObject.gameObject);
                                // Then, unregistering it.
                                objects.Remove(clientId);
                            }
                            Debug.Log($"ClientMovementProtocolServerSide.OnDisconnected({clientId})::End");
                        });
                    }

                    private void DoThrottled(ulong connectionId, Action<MapObject> callback)
                    {
                        ObjectOwnage ownage = objects[connectionId];
                        MapObject obj = ownage.OwnedObject.MapObject;
                        float time = Time.time;
                        if (ownage.LastCommandTime + 0.75 / obj.Speed <= time)
                        {
                            ownage.LastCommandTime = time;
                            callback(obj);
                        }
                    }

                    protected override void SetIncomingMessageHandlers()
                    {
                        AddIncomingMessageHandler("Move:Down", async (proto, connectionId) => {
                            var _ = RunInMainThread(() =>
                            {
                                DoThrottled(connectionId, (obj) =>
                                {
                                    obj.Orientation = Direction.DOWN;
                                    obj.StartMovement(Direction.DOWN);
                                });
                            });
                        });
                        AddIncomingMessageHandler("Move:Left", async (proto, connectionId) => {
                            var _ = RunInMainThread(() =>
                            {
                                DoThrottled(connectionId, (obj) =>
                                {
                                    obj.Orientation = Direction.LEFT;
                                    obj.StartMovement(Direction.LEFT);
                                });
                            });
                        });
                        AddIncomingMessageHandler("Move:Right", async (proto, connectionId) => {
                            var _ = RunInMainThread(() =>
                            {
                                DoThrottled(connectionId, (obj) =>
                                {
                                    obj.Orientation = Direction.RIGHT;
                                    obj.StartMovement(Direction.RIGHT);
                                });
                            });
                        });
                        AddIncomingMessageHandler("Move:Up", async (proto, connectionId) => {
                            var _ = RunInMainThread(() =>
                            {
                                DoThrottled(connectionId, (obj) =>
                                {
                                    obj.Orientation = Direction.UP;
                                    obj.StartMovement(Direction.UP);
                                });
                            });
                        });
                    }
                }
            }
        }
    }
}