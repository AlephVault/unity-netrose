using GameMeanMachine.Unity.WindRose.Authoring.Behaviours.Entities.Objects;
using GameMeanMachine.Unity.WindRose.Authoring.Behaviours.Entities.Objects.Teleport;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameMeanMachine.Unity.NetRose
{
    namespace Samples
    {
        namespace Server
        {
            /// <summary>
            ///   This is just a door (a teleporter + teleport target, with
            ///   a forced movement on arrival).
            /// </summary>
            [RequireComponent(typeof(TeleportTarget))]
            public class Door : LocalTeleporter
            {
                protected override void DoTeleport(Action teleport, MapObject objectToBeTeleported, TeleportTarget teleportTarget, MapObject teleportTargetObject)
                {
                    base.DoTeleport(teleport, objectToBeTeleported, teleportTarget, teleportTargetObject);
                    if (teleportTarget.ForceOrientation)
                    {
                        objectToBeTeleported.StartMovement(teleportTarget.NewOrientation);
                    }
                }
            }
        }
    }
}