using AlephVault.Unity.Binary;
using GameMeanMachine.Unity.WindRose.Types;

namespace GameMeanMachine.Unity.NetRose
{
    namespace Types
    {
        namespace Models
        {
            /// <summary>
            ///   A map object model has two properties: the underlying
            ///   data and the attachment (which may be null when the
            ///   object is not attached to a map, and includes the
            ///   current movement).
            /// </summary>
            public class MapObjectModel<ModelData> : ISerializable
                where ModelData : class, ISerializable, new()
            {
                /// <summary>
                ///   The current map status. It is null if the object
                ///   is not attached to a map.
                /// </summary>
                public Status Status;

                /// <summary>
                ///   The current object data.
                /// </summary>
                public ModelData Data;

                /// <summary>
                ///   The current object orientation.
                /// </summary>
                public Direction Orientation;

                /// <summary>
                ///   The current object speed.
                /// </summary>
                public uint Speed;

                public void Serialize(Serializer serializer)
                {
                    if (serializer.IsReading)
                    {
                        if (serializer.Reader.ReadBool())
                        {
                            Status = new Status();
                            Status.Serialize(serializer);
                        }
                        else
                        {
                            Status = null;
                        }

                        serializer.Serialize(ref Orientation);
                        serializer.Serialize(ref Speed);

                        if (serializer.Reader.ReadBool())
                        {
                            Data = new ModelData();
                            Data.Serialize(serializer);
                        }
                        else
                        {
                            Data = null;
                        }
                    }
                    else
                    {
                        serializer.Writer.WriteBool(Status != null);
                        Status?.Serialize(serializer);
                        serializer.Serialize(ref Orientation);
                        serializer.Serialize(ref Speed);
                        serializer.Writer.WriteBool(Data != null);
                        Data?.Serialize(serializer);
                    }
                }

                public override string ToString()
                {
                    string statusStr = "detached";
                    if (Status != null)
                    {
                        string movementStr = Status.Movement?.ToString() ?? "staying";
                        string attachmentStr = $"[MapIdx={Status.Attachment.MapIndex}, coords=({Status.Attachment.Position.X}, {Status.Attachment.Position.Y})]";
                        statusStr = $"[Attachment={attachmentStr}, Movement={movementStr}]";
                    }
                    return $"[Status={statusStr}, Speed={Speed}, Orientation={Orientation}, Data={Data}]";
                }
            }
        }
    }
}
