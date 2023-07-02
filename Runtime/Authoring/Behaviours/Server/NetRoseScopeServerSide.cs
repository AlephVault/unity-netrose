using AlephVault.Unity.Meetgard.Scopes.Authoring.Behaviours.Server;
using AlephVault.Unity.WindRose.Authoring.Behaviours.World;
using AlephVault.Unity.WindRose.Types;
using System.Collections.Generic;
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
                ///   The scope server side of a netrose scope knows how
                ///   to reach the related client scope and also all the
                ///   involved WindRose maps and connections. It also
                ///   provides means to forward all the NetRose related
                ///   messages from objects to the protocol.
                /// </summary>
                [RequireComponent(typeof(ScopeServerSide))]
                [RequireComponent(typeof(Scope))]
                public class NetRoseScopeServerSide : MonoBehaviour
                {
                    /// <summary>
                    ///   The related scope server side (to get the connections and objects).
                    /// </summary>
                    public ScopeServerSide ScopeServerSide { get; private set; }

                    /// <summary>
                    ///   The protocol this NetRose scope is related to.
                    /// </summary>
                    public NetRoseProtocolServerSide NetRoseProtocolServerSide { get; private set; }

                    /// <summary>
                    ///   The related world scope (to get the maps).
                    /// </summary>
                    public Scope Maps { get; private set; }

                    /// <summary>
                    ///   The default scope name, for when we want these scopes to be
                    ///   added to the named scopes list. This is ignored for scopes
                    ///   loaded as extra.
                    /// </summary>
                    [SerializeField]
                    private string defaultName;

                    /// <summary>
                    ///   See <see cref="defaultName" />.
                    /// </summary>
                    public string DefaultName => defaultName;
                    
                    /// <summary>
                    ///   The current scope name. Derived directly from the default name,
                    ///   if the scope is instantiated by default, or explicitly chosen
                    ///   after loading, if the scope comes from the extra templates. 
                    /// </summary>
                    public string Name { get; internal set; }

                    private void Awake()
                    {
                        ScopeServerSide = GetComponent<ScopeServerSide>();
                        Maps = GetComponent<Scope>();
                        ScopeServerSide.OnLoad += ScopeServerSide_OnLoad;
                        ScopeServerSide.OnUnload += ScopeServerSide_OnUnload;
                    }

                    private void OnDestroy()
                    {
                        ScopeServerSide.OnLoad -= ScopeServerSide_OnLoad;
                        ScopeServerSide.OnUnload -= ScopeServerSide_OnUnload;
                    }

                    private async Task ScopeServerSide_OnLoad()
                    {
                        NetRoseProtocolServerSide = ScopeServerSide.Protocol.GetComponent<NetRoseProtocolServerSide>();
                    }

                    private async Task ScopeServerSide_OnUnload()
                    {
                        Name = "";
                    }

                    /// <summary>
                    ///   Registers this scope into the named scopes by assigning
                    ///   them a particular name.
                    /// </summary>
                    /// <param name="name"></param>
                    /// <returns></returns>
                    public bool RegisterNamedScope(string name)
                    {
                        // Reject if the name is empty.
                        if ((name ?? "").Trim().Length == 0) return false;

                        // Reject if the scope is not loaded, or is default.
                        if (ScopeServerSide.Id == 0 || ScopeServerSide.PrefabId == 0 ||
                            ScopeServerSide.PrefabId == AlephVault.Unity.Meetgard.Scopes.Types.Constants.Scope.DefaultPrefab) return false;

                        // Reject if the world is not loaded (and this reference was
                        // kept alive after the entire world was unloaded).
                        if (!NetRoseProtocolServerSide.AreNamedScopesReady) return false;
                        
                        // Reject if the scope already has a name.
                        if (Name != "") return false;

                        // Reject if the name is already in use.
                        if (!NetRoseProtocolServerSide.AddNamedScope(name, this)) return false;

                        return true;
                    }

                    /// <summary>
                    ///   The scope server side id.
                    /// </summary>
                    public uint Id { get { return ScopeServerSide.Id; } }

                    /// <summary>
                    ///   Returns an iterator of all the objects in the scope.
                    /// </summary>
                    /// <returns>The iterator</returns>
                    public IEnumerable<ObjectServerSide> Objects()
                    {
                        return ScopeServerSide.Objects();
                    }

                    /// <summary>
                    ///   Returns an iterator of all the connections in the scope.
                    /// </summary>
                    /// <param name="except">Excludes some connection to be returned</param>
                    /// <returns>The iterator of connection ids</returns>
                    public IEnumerable<ulong> Connections(ISet<ulong> except = null)
                    {
                        return ScopeServerSide.Connections(except);
                    }

                    // Now, the internal protocol methods are to be defined here.
                    // These are the same internal methods in the protocol server side,
                    // but without specifying the connections.

                    /// <summary>
                    ///   Broadcasts a "object attached" message. This message is triggered when
                    ///   an object is added to a map in certain scope.
                    /// </summary>
                    /// <param name="objectId">The id of the object</param>
                    /// <param name="mapIndex">The index of the map, inside the scope, this object is being added to</param>
                    /// <param name="x">The x position of the object in the new map</param>
                    /// <param name="y">The y position of the object in the new map</param>
                    internal async Task BroadcastObjectAttached(uint objectId, uint mapIndex, ushort x, ushort y)
                    {
                        await NetRoseProtocolServerSide.BroadcastObjectAttached(Connections(), ScopeServerSide.Id, objectId, mapIndex, x, y);
                    }

                    /// <summary>
                    ///   Broadcasts a "object attached" message. This message is triggered when
                    ///   an object is added to a map in certain scope.
                    /// </summary>
                    /// <param name="objectId">The id of the object</param>
                    internal async Task BroadcastObjectDetached(uint objectId)
                    {
                        await NetRoseProtocolServerSide.BroadcastObjectDetached(Connections(), ScopeServerSide.Id, objectId);
                    }

                    /// <summary>
                    ///   Broadcasts a "object movement started" message. This message is triggered when
                    ///   an object started moving inside a map in the current scope.
                    /// </summary>
                    /// <param name="objectId">The id of the object</param>
                    /// <param name="x">The starting x position of the object when starting movement</param>
                    /// <param name="y">The starting y position of the object when starting movement</param>
                    /// <param name="direction">The direction of the object when starting movement</param>
                    internal async Task BroadcastObjectMovementStarted(uint objectId, ushort x, ushort y, Direction direction)
                    {
                        IEnumerable<ulong> connections = null;
                        try
                        {
                            connections = Connections();
                        }
                        catch(Exception e)
                        {
                            Debug.LogException(e);
                        }
                        await NetRoseProtocolServerSide.BroadcastObjectMovementStarted(connections, ScopeServerSide.Id, objectId, x, y, direction);
                    }

                    /// <summary>
                    ///   Broadcasts a "object movement cancelled" message. This message is triggered when
                    ///   an object cancelled moving inside a map in certain scope.
                    /// </summary>
                    /// <param name="objectId">The id of the object</param>
                    /// <param name="x">The to-revert x position of the object when cancelling movement</param>
                    /// <param name="y">The to-revert y position of the object when cancelling movement</param>
                    internal async Task BroadcastObjectMovementCancelled(uint objectId, ushort x, ushort y)
                    {
                        await NetRoseProtocolServerSide.BroadcastObjectMovementCancelled(Connections(), ScopeServerSide.Id, objectId, x, y);
                    }

                    /// <summary>
                    ///   Broadcasts a "object movement finished" message. This message is triggered when
                    ///   an object finished moving inside a map in certain scope.
                    /// </summary>
                    /// <param name="objectId">The id of the object</param>
                    /// <param name="x">The end x position of the object when finishing movement</param>
                    /// <param name="y">The end y position of the object when finishing movement</param>
                    internal async Task BroadcastObjectMovementFinished(uint objectId, ushort x, ushort y)
                    {
                        await NetRoseProtocolServerSide.BroadcastObjectMovementFinished(Connections(), ScopeServerSide.Id, objectId, x, y);
                    }

                    /// <summary>
                    ///   Broadcasts a "object movement teleported" message. This message is triggered when
                    ///   an object teleported inside a map in certain scope.
                    /// </summary>
                    /// <param name="objectId">The id of the object</param>
                    /// <param name="x">The end x position of the object when teleporting</param>
                    /// <param name="y">The end y position of the object when teleporting</param>
                    internal async Task BroadcastObjectTeleported(uint objectId, ushort x, ushort y)
                    {
                        await NetRoseProtocolServerSide.BroadcastObjectTeleported(Connections(), ScopeServerSide.Id, objectId, x, y);
                    }

                    /// <summary>
                    ///   Broadcasts a "object speed changed" message. This message is triggered when
                    ///   an object changed its speed inside a map in certain scope.
                    /// </summary>
                    /// <param name="objectId">The id of the object</param>
                    /// <param name="speed">The new object speed</param>
                    internal async Task BroadcastObjectSpeedChanged(uint objectId, uint speed)
                    {
                        await NetRoseProtocolServerSide.BroadcastObjectSpeedChanged(Connections(), ScopeServerSide.Id, objectId, speed);
                    }

                    /// <summary>
                    ///   Broadcasts a "object orientation changed" message. This message is triggered when
                    ///   an object changed its orientation inside a map in certain scope.
                    /// </summary>
                    /// <param name="objectId">The id of the object</param>
                    /// <param name="orientation">The new object orientation</param>
                    internal async Task BroadcastObjectOrientationChanged(uint objectId, Direction orientation)
                    {
                        await NetRoseProtocolServerSide.BroadcastObjectOrientationChanged(Connections(), ScopeServerSide.Id, objectId, orientation);
                    }
                    
                    /// <summary>
                    ///   Sends a "object movement rejected" message. This message is triggered when
                    ///   an object is rejected from moving inside a map in certain scope.
                    /// </summary>
                    /// <param name="connectionId">The connection id to send this message to</param>
                    /// <param name="objectId">The id of the object</param>
                    /// <param name="x">The to-revert x position of the object when cancelling movement</param>
                    /// <param name="y">The to-revert y position of the object when cancelling movement</param>
                    internal async Task SendObjectMovementRejected(ulong connectionId, uint objectId, ushort x, ushort y)
                    {
                        await NetRoseProtocolServerSide.SendObjectMovementRejected(connectionId, ScopeServerSide.Id, objectId, x, y);
                    }
                }
            }
        }
    }
}
