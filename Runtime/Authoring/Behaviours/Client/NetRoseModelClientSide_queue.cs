using AlephVault.Unity.Binary;
using AlephVault.Unity.Meetgard.Scopes.Authoring.Behaviours.Client;
using GameMeanMachine.Unity.NetRose.Types.Models;
using GameMeanMachine.Unity.WindRose.Authoring.Behaviours.Entities.Objects;
using GameMeanMachine.Unity.WindRose.Types;
using System.Collections.Generic;
using UnityEngine;


namespace GameMeanMachine.Unity.NetRose
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace Client
            {
                public abstract partial class NetRoseModelClientSide<SpawnData, RefreshData>
                    where SpawnData : class, ISerializable, new()
                    where RefreshData : class, ISerializable, new()
                {
                    /// <summary>
                    ///   One of the queued commands: Movement Start, Finish, Cancel,
                    ///   Speed Change, and Orientation Change.
                    /// </summary>
                    private interface QueuedCommand
                    {
                        /// <summary>
                        ///   Tells whether the command can start immediately or not.
                        /// </summary>
                        /// <param name="force">Whether it is running in forced mode or not</param>
                        /// <returns>Whether it can execute or not</returns>
                        public abstract bool CanExecute(bool force);

                        /// <summary>
                        ///   Executes the command on a given <see cref="MapObject"/>.
                        /// </summary>
                        /// <param name="obj">The object to execute the command into</param>
                        public abstract void Execute(MapObject obj);
                    }

                    /// <summary>
                    ///   A Movement Start queued command.
                    /// </summary>
                    private class MovementStartCommand : QueuedCommand
                    {
                        /// <summary>
                        ///   The start x-position.
                        /// </summary>
                        public ushort StartX;

                        /// <summary>
                        ///   The start y-position.
                        /// </summary>
                        public ushort StartY;

                        /// <summary>
                        ///   The movement direction.
                        /// </summary>
                        public Direction Direction;

                        /// <inheritdoc />
                        public bool CanExecute(bool force)
                        {
                            return force;
                        }

                        /// <inheritdoc />
                        public void Execute(MapObject obj)
                        {
                            if (obj.X != StartX || obj.Y != StartY) obj.Teleport(StartX, StartY, true);
                            obj.StartMovement(Direction);
                        }
                    }

                    /// <summary>
                    ///   A Movement Cancel queued command.
                    /// </summary>
                    private class MovementCancelCommand : QueuedCommand
                    {
                        /// <summary>
                        ///   The x-position to revert on cancel.
                        /// </summary>
                        public ushort RevertX;

                        /// <summary>
                        ///   The y-position to revert on cancel.
                        /// </summary>
                        public ushort RevertY;

                        /// <inheritdoc />
                        public bool CanExecute(bool force)
                        {
                            return true;
                        }

                        /// <inheritdoc />
                        public void Execute(MapObject obj)
                        {
                            obj.CancelMovement();
                            if (obj.X != RevertX || obj.Y != RevertY) obj.Teleport(RevertX, RevertY, true);
                        }
                    }

                    /// <summary>
                    ///   A Movement Finish queued command.
                    /// </summary>
                    private class MovementFinishCommand : QueuedCommand
                    {
                        /// <summary>
                        ///   The end x-position.
                        /// </summary>
                        public ushort EndX;

                        /// <summary>
                        ///   The end y-position.
                        /// </summary>
                        public ushort EndY;

                        /// <inheritdoc />
                        public bool CanExecute(bool force)
                        {
                            return force;
                        }

                        /// <inheritdoc />
                        public void Execute(MapObject obj)
                        {
                            obj.FinishMovement();
                            if (obj.X != EndX || obj.Y != EndY) obj.Teleport(EndX, EndY, true);
                        }
                    }

                    /// <summary>
                    ///   A Speed Change queued command.
                    /// </summary>
                    private class SpeedChangeCommand : QueuedCommand
                    {
                        /// <summary>
                        ///   The new speed.
                        /// </summary>
                        public uint Speed;

                        /// <inheritdoc />
                        public bool CanExecute(bool force)
                        {
                            return true;
                        }

                        /// <inheritdoc />
                        public void Execute(MapObject obj)
                        {
                            obj.Speed = Speed;
                        }
                    }

                    /// <summary>
                    ///   An Orientation Change queued command.
                    /// </summary>
                    private class OrientationChangeCommand : QueuedCommand
                    {
                        /// <summary>
                        ///   The new orientation.
                        /// </summary>
                        public Direction Orientation;

                        /// <inheritdoc />
                        public bool CanExecute(bool force)
                        {
                            return true;
                        }

                        /// <inheritdoc />
                        public void Execute(MapObject obj)
                        {
                            obj.Orientation = Orientation;
                        }
                    }

                    // This is the list of the queued commands.
                    private List<QueuedCommand> queue = new List<QueuedCommand>();

                    // This tells whether the queue is currently running or not.
                    private bool runningQueue = false;

                    // Queues a command in the queue, and runs the queue
                    // either in regular mode or in accelerated mode,
                    // depending on whether the queue was not full, or
                    // was full.
                    private void QueueElement(QueuedCommand command)
                    {
                        bool full = queue.Count >= lagTolerance;
                        queue.Add(command);
                        RunQueue(full);
                    }
                    
                    // Runs the queue, all that it can. Either if it is
                    // free to move, or the whole call is accelerated.
                    private void RunQueue(bool accelerated)
                    {
                        try
                        {
                            runningQueue = true;
                            bool freeToMove = !MapObject.IsMoving;
                            while (queue.Count > 0 && queue[0].CanExecute(freeToMove || accelerated))
                            {
                                queue[0].Execute(MapObject);
                                if (queue[0] is MovementStartCommand)
                                {
                                    freeToMove = false;
                                }
                                queue.RemoveAt(0);
                            }
                        }
                        finally
                        {
                            runningQueue = false;
                        }
                    }
                }
            }
        }
    }
}
