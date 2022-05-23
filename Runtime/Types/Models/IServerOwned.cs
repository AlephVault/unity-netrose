namespace GameMeanMachine.Unity.NetRose
{
    namespace Types
    {
        namespace Models
        {
            /// <summary>
            ///   Used to tell which connection owns the object.
            ///   Objects not implementing this interface should
            ///   be considered as not having owner.
            /// </summary>
            public interface IServerOwned
            {
                /// <summary>
                ///   Sets the owner (connection id) of the object.
                ///   0 means "no owner".
                /// </summary>
                internal void SetOwner(ulong connectionId);
                
                /// <summary>
                ///   Gets the owner (connection id) of the object.
                ///   0 means "no owner". Weird case, however, and
                ///   not part of the principal dict.
                /// </summary>
                public ulong GetOwner();
            }
        }
    }
}