namespace AlephVault.Unity.NetRose
{
    namespace Types
    {
        namespace Protocols
        {
            namespace Messages
            {
                using AlephVault.Unity.Binary;
                using AlephVault.Unity.NetRose.Types.Models;
                using AlephVault.Unity.WindRose.Types;

                /// <summary>
                ///   <para>
                ///     Denotes the start of a movement (start position and direction).
                ///   </para>
                /// </summary>
                public struct MovementStart : ISerializable
                {
                    /// <summary>
                    ///   The initial position of the movement.
                    /// </summary>
                    public Position Position;

                    /// <summary>
                    ///   The direction of the started movement.
                    /// </summary>
                    public Direction Direction;

                    public void Serialize(Serializer serializer)
                    {
                        Position.Serialize(serializer);
                        serializer.Serialize(ref Direction);
                    }
                }
            }
        }
    }
}
