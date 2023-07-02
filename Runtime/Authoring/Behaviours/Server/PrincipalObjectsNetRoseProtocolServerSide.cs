using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlephVault.Unity.Meetgard.Scopes.Authoring.Behaviours.Server;
using AlephVault.Unity.NetRose.Types.Models;
using AlephVault.Unity.NetRose.Types.Protocols.Messages;
using AlephVault.Unity.WindRose.Authoring.Behaviours.Entities.Objects;
using AlephVault.Unity.WindRose.Authoring.Behaviours.World;
using AlephVault.Unity.WindRose.Types;
using UnityEngine;
using Exception = System.Exception;


namespace AlephVault.Unity.NetRose
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace Server
            {
                /// <summary>
                ///   <para>
                ///     This is a particular subclass of <see cref="NetRoseProtocolServerSide"/>
                ///     that provides methods to handle creation-and-assignment of owned objects
                ///     for a connection (and start syncing the current scope) and also handle
                ///     the de-assignment-and-destruction of o owned objects (and sending the
                ///     former owners to Limbo). When those features are used... only depends
                ///     on the developer's needs.
                ///   </para>
                ///   <para>
                ///     It also provides functions to get map references by (scope, index).
                ///   </para>
                /// </summary>
                public class PrincipalObjectsNetRoseProtocolServerSide<T> : NetRoseProtocolServerSide
                    where T : ObjectServerSide, IServerOwned, INetRoseModelServerSide
                {
                    protected const string UseDefaultKey = "";
                    protected const int UseDefaultIndex = -1;

                    // Tracks NetRose scopes by their key. Empty or empty-like strings will
                    // never be present by this mean. Also, depending on the scope type
                    private Dictionary<string, NetRoseScopeServerSide> scopesByKey =
                        new Dictionary<string, NetRoseScopeServerSide>();

                    // All the principal objects are meant to be tracked here..
                    private Dictionary<ulong, T> objects = new Dictionary<ulong, T>();

                    /// <summary>
                    ///   When using a negative index in
                    ///   <see cref="InstantiatePrincipal(ulong,int,Map,ushort,ushort,Action{T},Action{T})"/>,
                    ///   this index will be used instead to get a prefab for the principal
                    ///   object for a connection.
                    /// </summary>
                    [SerializeField]
                    private int defaultPrefabIndex = -1;

                    /// <summary>
                    ///   When using an empty or null key in
                    ///   <see cref="InstantiatePrincipal(ulong,string,Map,ushort,ushort,Action{T},Action{T})"/>,
                    ///   this key will be used instead to get a prefab for the principal
                    ///   object for a connection.
                    /// </summary>
                    [SerializeField]
                    private string defaultPrefabKey = "";
                    
                    /// <summary>
                    ///   Initializes the principal objects dictionary.
                    /// </summary>
                    public override async Task OnServerStarted()
                    {
                        objects = new Dictionary<ulong, T>();
                    }

                    /// <summary>
                    ///   Clears the principal objects dictionary.
                    /// </summary>
                    public override async Task OnServerStopped(Exception e)
                    {
                        objects = null;
                    }

                    /// <summary>
                    ///   Given a connection id, it takes a prefab index to instantiate a new
                    ///   object for the connection and arguments to attach and initialize the
                    ///   object.
                    /// </summary>
                    /// <param name="connectionId">The ID of the connection to instantiate the object to</param>
                    /// <param name="prefabIndex">
                    ///   The index of the prefab to use. If lower than 0, <see cref="defaultPrefabIndex"/>
                    ///   will be considered instead.
                    /// </param>
                    /// <param name="toMap">
                    ///   A map to attach the object to. This map should never be null but, if missing,
                    ///   then no attachment will be done to the map. Also, this map should belong to
                    ///   a synchronized scope, otherwise the clients will get nothing rendered. This
                    ///   is actually a callback that retrieves the map.
                    /// </param>
                    /// <param name="x">The x position in the new map</param>
                    /// <param name="y">The y position in the new map</param>
                    /// <param name="beforeAttach">Optional callback to invoke when the object is awakened</param>
                    /// <param name="afterAttach">
                    ///   Optional callback to invoke after the object is attached (even if the map is
                    ///   null and no attachment is actually done, this callback is still invoked).
                    /// </param>
                    protected void InstantiatePrincipal(
                        ulong connectionId, int prefabIndex, Func<Map> toMap, ushort x, ushort y,
                        Action<T> beforeAttach = null, Action<T> afterAttach = null
                    )
                    {
                        if (prefabIndex < 0)
                        {
                            prefabIndex = defaultPrefabIndex;
                        }
                        // Yes: it is OK to test again the same condition.
                        if (prefabIndex < 0)
                        {
                            throw new ArgumentException(
                                "A non-negative default prefab index must be configured when " +
                                "a negative prefab index is specified as argument"
                            );
                        }

                        _ = RunInMainThread(() =>
                        {
                            try
                            {
                                // Check no principal object exists for the connection.
                                CheckNotAlreadyHavingPrincipal(connectionId);
                                // Instantiate the object.
                                T behaviour = null;
                                ObjectServerSide obj = InstantiateHere((uint)prefabIndex, (obj) => {
                                    InitObj(obj, beforeAttach, connectionId);
                                    behaviour = obj.GetComponent<T>();
                                    objects[connectionId] = behaviour;
                                });
                                AttachObj(behaviour, toMap(), x, y, afterAttach);
                            }
                            catch (Exception e)
                            {
                                Debug.LogException(e);
                                throw;
                            }
                        });
                    }
                    
                    /// <summary>
                    ///   Given a connection id, it takes a prefab key to instantiate a new
                    ///   object for the connection and arguments to attach and initialize the
                    ///   object.
                    /// </summary>
                    /// <param name="connectionId">The ID of the connection to instantiate the object to</param>
                    /// <param name="prefabKey">
                    ///   The key of the prefab to use. If null or empty, <see cref="defaultPrefabKey"/>
                    ///   will be considered instead.
                    /// </param>
                    /// <param name="toMap">
                    ///   A map to attach the object to. This map should never be null but, if missing,
                    ///   then no attachment will be done to the map. Also, this map should belong to
                    ///   a synchronized scope, otherwise the clients will get nothing rendered. This
                    ///   is actually a callback that retrieves the map.
                    /// </param>
                    /// <param name="x">The x position in the new map</param>
                    /// <param name="y">The y position in the new map</param>
                    /// <param name="beforeAttach">Optional callback to invoke when the object is awakened</param>
                    /// <param name="afterAttach">
                    ///   Optional callback to invoke after the object is attached (even if the map is
                    ///   null and no attachment is actually done, this callback is still invoked).
                    /// </param>
                    protected void InstantiatePrincipal(
                        ulong connectionId, string prefabKey, Func<Map> toMap, ushort x, ushort y,
                        Action<T> beforeAttach = null, Action<T> afterAttach = null
                    )
                    {
                        if (string.IsNullOrWhiteSpace(prefabKey))
                        {
                            prefabKey = defaultPrefabKey;
                        }
                        // Yes: it is OK to test again the same condition.
                        if (string.IsNullOrWhiteSpace(prefabKey))
                        {
                            throw new ArgumentException(
                                "A non-whitespace default prefab key must be configured when " +
                                "a null/whitespace key is specified as argument"
                            );
                        }
                        
                        _ = RunInMainThread(() =>
                        {
                            try
                            {
                                // Check no principal object exists for the connection.
                                CheckNotAlreadyHavingPrincipal(connectionId);
                                // Instantiate the object.
                                T behaviour = null;
                                ObjectServerSide obj = InstantiateHere(prefabKey, (obj) => {
                                    InitObj(obj, beforeAttach, connectionId);
                                    behaviour = obj.GetComponent<T>();
                                    objects[connectionId] = behaviour;
                                });
                                AttachObj(behaviour, toMap(), x, y, afterAttach);
                            }
                            catch (Exception e)
                            {
                                Debug.LogException(e);
                                throw;
                            }
                        });
                    }

                    // Ensures the object has the behaviour T and initializes it.
                    private void InitObj(ObjectServerSide obj, Action<T> beforeAttach, ulong connectionId)
                    {
                        // Get the behaviour.
                        T t = obj.GetComponent<T>();
                        if (!t)
                        {
                            throw new ArgumentException(
                                "The chosen prefab does not have a required behaviour of " +
                                $"type {typeof(T).FullName}"
                            );
                        }
                        
                        // Set the owner of the object.
                        t.SetOwner(connectionId);
                        
                        // Invoke the custom initialization.
                        try
                        {
                            beforeAttach?.Invoke(t);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning(
                                $"An exception occurred while initializing a principal!\n" +
                                e.StackTrace
                            );
                        }
                    }

                    /// <summary>
                    ///   Removes the principal object from a connection. This will
                    ///   cause the connection to go to Limbo.
                    /// </summary>
                    /// <param name="connectionId">The connection id whose principal is being destroyed</param>
                    protected void RemovePrincipal(ulong connectionId)
                    {
                        _ = RunInMainThread(() =>
                        {
                            // Check no principal object exists for the connection.
                            CheckAlreadyHavingPrincipal(connectionId);
                            Destroy(objects[connectionId].gameObject);
                            objects.Remove(connectionId);
                        });
                    }

                    /// <summary>
                    ///   Gets a principal for a connection.
                    /// </summary>
                    /// <param name="connectionId">The connection to get the principal for</param>
                    /// <returns>The principal object for the connection</returns>
                    public T GetPrincipal(ulong connectionId)
                    {
                        try
                        {
                            return objects[connectionId];
                        }
                        catch (KeyNotFoundException e)
                        {
                            throw new ArgumentException(
                                $"The connection with id: {connectionId} does not " +
                                $"have a principal object yet"
                            );
                        }
                    }

                    /// <summary>
                    ///   Tells whether there is a principal for a given connection id.
                    /// </summary>
                    /// <param name="connectionId">The connection to check the principal for</param>
                    /// <returns>Whether a principal exists for that connection id</returns>
                    public bool HasPrincipal(ulong connectionId)
                    {
                        return objects.ContainsKey(connectionId);
                    }

                    /// <summary>
                    ///   Tries getting the principal for a given connection id.
                    /// </summary>
                    /// <param name="connectionId">The connection to get the principal for</param>
                    /// <param name="principal">The obtained principal</param>
                    /// <returns>Whether a principal exists for that connection id</returns>
                    public bool TryGetPrincipal(ulong connectionId, out T principal)
                    {
                        return objects.TryGetValue(connectionId, out principal);
                    }

                    // Attaches the object to a new map, and invokes the afterAttach callback.
                    private void AttachObj(T obj, Map toMap, ushort x, ushort y, Action<T> afterAttach)
                    {
                        // Attach the object.
                        if (toMap)
                        {
                            obj.MapObject.Attach(toMap, x, y, true);
                        }
                        
                        // Invoke the custom initialization.
                        try
                        {
                            afterAttach?.Invoke(obj);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning(
                                $"An exception occurred while initializing a principal!\n" +
                                e.StackTrace
                            );
                        }
                    }

                    // Ensures the connection doesn't already have a principal object.
                    private void CheckNotAlreadyHavingPrincipal(ulong connectionId)
                    {
                        try
                        {
                            T obj = objects[connectionId];
                            // This tests whether the object is ALIVE vs.
                            // the object is DESTROYED. Not just null values.
                            if (obj) throw new InvalidOperationException(
                                $"The connection with id: {connectionId} already has " +
                                $"an object instantiated as its principal"
                            );
                        }
                        catch (KeyNotFoundException e)
                        {
                        }
                    }
                    
                    // Ensures the connection already has a principal object.
                    private void CheckAlreadyHavingPrincipal(ulong connectionId)
                    {
                        try
                        {
                            T obj = objects[connectionId];
                            // This tests whether the object is ALIVE vs.
                            // the object is DESTROYED. Not just null values.
                            if (!obj) throw new InvalidOperationException(
                                $"The connection with id: {connectionId} does not " +
                                $"have an object instantiated as its principal"
                            );
                        }
                        catch (KeyNotFoundException e)
                        {
                            throw new InvalidOperationException(
                                $"The connection with id: {connectionId} does not " +
                                $"have an object instantiated as its principal"
                            );
                        }
                    }

                    // Attempts moving an object. If it is already moving, returns
                    // false. If it is NOT moving, attempts the movement. If the
                    // movement is not successful and this is an optimistic call
                    // (i.e. for games using optimistic objects in the client side)
                    // then a MovementRejected message will be sent to the client.
                    // Anyway, the result is true in either case.
                    private void Move(ulong connectionId, Direction direction, bool queue)
                    {
                        T principal = GetPrincipal(connectionId);
                        MapObject obj = principal.MapObject;
                        _ = RunInMainThread(() =>
                        {
                            obj.Orientation = direction;
                            obj.StartMovement(direction, obj.IsMoving, queue);
                        });
                    }

                    /// <summary>
                    ///   Attempts moving the owned object to the left.
                    /// </summary>
                    /// <param name="connectionId">The connection id to move the object for</param>
                    /// <returns>Whether the object was moving or not</returns>
                    /// <remarks>
                    ///   This function MUST be called in the main thread
                    ///   (e.g. in a callback provided to RunInMainThread).
                    /// </remarks>
                    public void MoveLeft(ulong connectionId, bool queue = true)
                    {
                        Move(connectionId, Direction.LEFT, queue);
                    }
                    
                    /// <summary>
                    ///   Attempts moving the owned object to the left.
                    /// </summary>
                    /// <param name="connectionId">The connection id to move the object for</param>
                    /// <returns>Whether the object was moving or not</returns>
                    /// <remarks>
                    ///   This function MUST be called in the main thread
                    ///   (e.g. in a callback provided to RunInMainThread).
                    /// </remarks>
                    public void MoveRight(ulong connectionId, bool queue = true)
                    {
                        Move(connectionId, Direction.RIGHT, queue);
                    }

                    /// <summary>
                    ///   Attempts moving the owned object up.
                    /// </summary>
                    /// <param name="connectionId">The connection id to move the object for</param>
                    /// <returns>Whether the object was moving or not</returns>
                    /// <remarks>
                    ///   This function MUST be called in the main thread
                    ///   (e.g. in a callback provided to RunInMainThread).
                    /// </remarks>
                    public void MoveUp(ulong connectionId, bool queue = true)
                    {
                        Move(connectionId, Direction.UP, queue);
                    }

                    /// <summary>
                    ///   Attempts moving the main object down.
                    /// </summary>
                    /// <param name="connectionId">The connection id to move the object for</param>
                    /// <returns>Whether the object was moving or not</returns>
                    /// <remarks>
                    ///   This function MUST be called in the main thread
                    ///   (e.g. in a callback provided to RunInMainThread).
                    /// </remarks>
                    public void MoveDown(ulong connectionId, bool queue = true)
                    {
                        Move(connectionId, Direction.DOWN, queue);
                    }
                }
            }
        }
    }
}