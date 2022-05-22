using System;
using System.Threading.Tasks;
using UnityEngine;
using GameMeanMachine.Unity.NetRose.Samples.Common.Protocols;
using AlephVault.Unity.Meetgard.Authoring.Behaviours.Client;
using GameMeanMachine.Unity.NetRose.Authoring.Behaviours.Client;
using AlephVault.Unity.Support.Authoring.Behaviours;

namespace GameMeanMachine.Unity.NetRose
{
    namespace Samples
    {
        namespace Client
        {
            namespace Protocols
            {
                [RequireComponent(typeof(Throttler))]
                [RequireComponent(typeof(PrincipalObjectsNetRoseProtocolClientSide))]
                public class ClientMovementProtocolClientSide : ProtocolClientSide<ClientMovementProtocolDefinition>
                {
                    private Throttler throttler;
                    private bool canWalk;
                    private Func<Task> moveDownSender;
                    private Func<Task> moveLeftSender;
                    private Func<Task> moveRightSender;
                    private Func<Task> moveUpSender;

                    protected override void Setup()
                    {
                        throttler = GetComponent<Throttler>();
                    }

                    protected override void Initialize()
                    {
                        moveDownSender = MakeSender("Move:Down");
                        moveLeftSender = MakeSender("Move:Left");
                        moveRightSender = MakeSender("Move:Right");
                        moveUpSender = MakeSender("Move:Up");
                    }

                    public override async Task OnConnected()
                    {
                        canWalk = true;
                    }

                    public override async Task OnDisconnected(System.Exception reason)
                    {
                        canWalk = false;
                    }

                    public void WalkDown()
                    {
                        if (canWalk) throttler.Throttled(() => { RunInMainThread(moveDownSender); });
                    }

                    public void WalkLeft()
                    {
                        if (canWalk) throttler.Throttled(() => { RunInMainThread(moveLeftSender); });
                    }

                    public void WalkRight()
                    {
                        if (canWalk) throttler.Throttled(() => { RunInMainThread(moveRightSender); });
                    }

                    public void WalkUp()
                    {
                        if (canWalk) throttler.Throttled(() => { RunInMainThread(moveUpSender); });
                    }
                }
            }
        }
    }
}