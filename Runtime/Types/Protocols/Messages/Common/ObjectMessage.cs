namespace GameMeanMachine.Unity.NetRose
{
    namespace Types
    {
        namespace Protocols
        {
            namespace Messages
            {
                using AlephVault.Unity.Binary;

                /// <summary>
                ///   <para>
                ///     An object-related message.
                ///   </para>
                /// </summary>
                public struct ObjectMessage<T> : ISerializable
                    where T : ISerializable, new()
                {
                    /// <summary>
                    ///   The scope the object belongs to.
                    /// </summary>
                    public uint ScopeId;

                    /// <summary>
                    ///   The object id.
                    /// </summary>
                    public uint ObjectId;

                    /// <summary>
                    ///   The message content.
                    /// </summary>
                    public T Content;

                    public void Serialize(Serializer serializer)
                    {
                        serializer.Serialize(ref ScopeId);
                        serializer.Serialize(ref ObjectId);
                        if (serializer.IsReading)
                        {
                            if (serializer.Reader.ReadBool())
                            {
                                Content = new T();
                                Content.Serialize(serializer);
                            }
                        }
                        else
                        {
                            serializer.Writer.WriteBool(Content != null);
                            Content?.Serialize(serializer);
                        }
                    }
                }
            }
        }
    }
}
