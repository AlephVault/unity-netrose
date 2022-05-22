using System;
using UnityEngine;
using AlephVault.Unity.Meetgard.Authoring.Behaviours.Server;
using GameMeanMachine.Unity.WindRose.Authoring.Behaviours.Entities.Objects;
using GameMeanMachine.Unity.WindRose.Types;
using GameMeanMachine.Unity.NetRose.Samples.Common.Protocols;
using Exception = System.Exception;


namespace GameMeanMachine.Unity.NetRose
{
    namespace Samples
    {
        namespace Server
        {
            namespace Protocols
            {
                [RequireComponent(typeof(SamplePrincipalProtocolServerSide))]
                public class ClientMovementProtocolServerSide : ProtocolServerSide<ClientMovementProtocolDefinition>
                {
                    private SamplePrincipalProtocolServerSide SamplePrincipalProtocolServerSide;

                    protected override void Setup()
                    {
                        SamplePrincipalProtocolServerSide = GetComponent<SamplePrincipalProtocolServerSide>();
                    }
                    
                    private void DoThrottled(ulong connectionId, Action<MapObject> callback)
                    {
                        try
                        {
                            OwnableModelServerSide ownage = SamplePrincipalProtocolServerSide.GetPrincipal(connectionId);
                            MapObject obj = ownage.MapObject;
                            float time = Time.time;
                            if (ownage.LastCommandTime + 0.75 / obj.Speed <= time)
                            {
                                ownage.LastCommandTime = time;
                                callback(obj);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
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