using AlephVault.Unity.Binary.Wrappers;
using AlephVault.Unity.Meetgard.Authoring.Behaviours.Client;
using AlephVault.Unity.Meetgard.Scopes.Authoring.Behaviours.Client;
using AlephVault.Unity.Meetgard.Types;
using AlephVault.Unity.Support.Utils;
using GameMeanMachine.Unity.NetRose.Types.Models;
using GameMeanMachine.Unity.NetRose.Types.Protocols;
using GameMeanMachine.Unity.NetRose.Types.Protocols.Messages;
using GameMeanMachine.Unity.WindRose.Authoring.Behaviours.World;
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
            namespace Client
            {
                /// <summary>
                ///   This is a particular subclass of <see cref="NetRoseProtocolClientSide"/>
                ///   which accounts for an object being the principal one (i.e. works with
                ///   the <see cref="OwnedNetRoseModelClientSide{SpawnData,RefreshData}"/> as
                ///   it is an <see cref="IClientOwned"/> object, mainly). It grants a camera
                ///   to them (by default, the <see cref="Camera.main"/> one, but this can be
                ///   overridden in subclasses).
                /// </summary>
                public class PrincipalObjectsNetRoseProtocolClientSide : NetRoseProtocolClientSide
                {
                    /// <summary>
                    ///   Returns the camera that will be used in the principal objects.
                    /// </summary>
                    public virtual Camera GetCamera()
                    {
                        return Camera.main;
                    }
                }
            }
        }
    }
}