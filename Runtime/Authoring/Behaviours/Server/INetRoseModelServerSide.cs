using AlephVault.Unity.Binary;
using AlephVault.Unity.Meetgard.Scopes.Authoring.Behaviours.Server;
using AlephVault.Unity.WindRose.Authoring.Behaviours.Entities.Objects;
using AlephVault.Unity.WindRose.Authoring.Behaviours.World;
using AlephVault.Unity.WindRose.Types;
using AlephVault.Unity.NetRose.Types.Models;
using System;
using System.Threading.Tasks;
using UnityEngine;


namespace AlephVault.Unity.NetRose
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
                ///   client side by means of the current scope. This interface
                ///   represents all the common methods for that networked object.
                /// </summary>
                public interface INetRoseModelServerSide
                {
                    /// <summary>
                    ///   The required WindRose MapObject componente.
                    /// </summary>
                    public MapObject MapObject { get; }
                }
            }
        }
    }
}
