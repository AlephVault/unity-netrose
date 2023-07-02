using AlephVault.Unity.Binary;
using AlephVault.Unity.Support.Generic.Types;


namespace AlephVault.Unity.NetRose
{
    namespace Types
    {
        namespace Models
        {
            /// <summary>
            ///   Represents the position of an object in a map.
            /// </summary>
            public struct Position : ISerializable, ICopy<Position>
            {
                /// <summary>
                ///   The object's current X position.
                /// </summary>
                public ushort X;

                /// <summary>
                ///   The object's current Y position.
                /// </summary>
                public ushort Y;

                /// <summary>
                ///   Copies the entire position data.
                /// </summary>
                /// <param name="deep">Whether to do it recursively or not (it is meaningless in this class)</param>
                /// <returns>A copy of the position</returns>
                public Position Copy(bool deep = false)
                {
                    return new Position()
                    {
                        X = X, Y = Y
                    };
                }

                public void Serialize(Serializer serializer)
                {
                    serializer.Serialize(ref X);
                    serializer.Serialize(ref Y);
                }
            }
        }
    }
}
