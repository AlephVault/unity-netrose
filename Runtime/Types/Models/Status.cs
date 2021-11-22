using AlephVault.Unity.Binary;
using AlephVault.Unity.Support.Generic.Types;
using GameMeanMachine.Unity.WindRose.Types;


namespace GameMeanMachine.Unity.NetRose
{
    namespace Types
    {
        namespace Models
        {
            /// <summary>
            ///   Represents the attachment of an object to a map, and the
            ///   current movement, if any. This status only belongs to the
            ///   map and also contains the movement.
            /// </summary>
            public class Status : ISerializable
            {
                /// <summary>
                ///   The current attachment.
                /// </summary>
                public Attachment Attachment;

                /// <summary>
                ///   The current direction the object is movement to
                ///   (or null if it is not moving).
                /// </summary>
                public Direction? Movement;

                public void Serialize(Serializer serializer)
                {
                    Attachment.Serialize(serializer);
                    if (serializer.IsReading)
                    {
                        // Read movement.
                        if (serializer.Reader.ReadBool())
                        {
                            Direction movement = Direction.FRONT;
                            serializer.Serialize(ref movement);
                            Movement = movement;
                        }
                        else
                        {
                            Movement = null;
                        }
                    }
                    else
                    {
                        // Write movement.
                        serializer.Writer.WriteBool(Movement != null);
                        if (Movement != null)
                        {
                            Direction movement = Movement.Value;
                            serializer.Serialize(ref movement);
                        }
                    }
                }
            }
        }
    }
}
