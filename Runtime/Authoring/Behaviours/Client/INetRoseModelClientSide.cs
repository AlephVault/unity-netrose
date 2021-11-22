using AlephVault.Unity.Meetgard.Scopes.Authoring.Behaviours.Client;
using GameMeanMachine.Unity.WindRose.Authoring.Behaviours.Entities.Objects;
using GameMeanMachine.Unity.WindRose.Authoring.Behaviours.World;
using GameMeanMachine.Unity.WindRose.Types;
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
                ///   Map objects exist in client side, reflecting what is received
                ///   from the server side, and applying some internal mechanisms
                ///   for lag balance and recovery. This interface represents all
                ///   the common methods for that networked object.
                /// </summary>
                public interface INetRoseModelClientSide
                {
                    /// <summary>
                    ///   The required WindRose MapObject component.
                    /// </summary>
                    public MapObject MapObject { get; }

                    /// <summary>
                    ///   Processes an attached event. This is immediate:
                    ///   Clears the queue (if it is executing, it stops),
                    ///   cancels the current movement (if any) and does
                    ///   the attachment.
                    /// </summary>
                    /// <param name="map">The map the object is being attached to</param>
                    /// <param name="x">The new x position</param>
                    /// <param name="y">The new y position</param>
                    internal void OnAttached(Map map, ushort x, ushort y);

                    /// <summary>
                    ///   Processes a detached event. This is immediate:
                    ///   Clears the queue (if it is executing, it stops),
                    ///   cancels the current movement (if any) and does
                    ///   the detachment.
                    /// </summary>
                    internal void OnDetached();

                    /// <summary>
                    ///   Processes a teleport event. This is immediate:
                    ///   Clears the queue (if it is executing, it stops),
                    ///   cancels the current movement (if any) and does
                    ///   the teleport.
                    /// </summary>
                    /// <param name="x">The new x position</param>
                    /// <param name="y">The new y position</param>
                    internal void OnTeleported(ushort x, ushort y);

                    /// <summary>
                    ///   Processes a movement start event. It queues the
                    ///   MovementStart command and, if the queue is not
                    ///   currently executing, it is now executed.
                    /// </summary>
                    /// <param name="x">The start x position</param>
                    /// <param name="y">The start y position</param>
                    /// <param name="direction">The movement direction</param>
                    internal void OnMovementStarted(ushort x, ushort y, Direction direction);

                    // Processes a movement cancel event. It queues the
                    // MovementCancel command and, if the queue is not
                    // currently executing, it is now executed.

                    internal void OnMovementCancelled(ushort x, ushort y);

                    // Processes a movement finish event. It queues the
                    // MovementFinish command and, if the queue is not
                    // currently executing, it is now executed.
                    internal void OnMovementFinished(ushort x, ushort y);

                    // Processes a movement speed change event. It queues
                    // the SpeedChanged command and, if the queue is not
                    // currently executing, it is now executed.
                    internal void OnSpeedChanged(uint speed);

                    // Processes an orientation change event. It queues the
                    // OrientationChanged command and, if the queue is not
                    // currently executing, it is now executed.
                    internal void OnOrientationChanged(Direction orientation);
                }
            }
        }
    }
}
