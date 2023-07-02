using AlephVault.Unity.Binary;


namespace AlephVault.Unity.NetRose
{
    namespace Types
    {
        namespace Models
        {
            /// <summary>
            ///   Represents the attachment to a map (index and position).
            /// </summary>
            public struct Attachment : ISerializable
            {
                /// <summary>
                ///   The index of the map the object is attached to.
                /// </summary>
                public uint MapIndex;

                /// <summary>
                ///   The object's current position.
                /// </summary>
                public Position Position;

                public void Serialize(Serializer serializer)
                {
                    serializer.Serialize(ref MapIndex);
                    Position.Serialize(serializer);
                }
            }
        }
    }
}
