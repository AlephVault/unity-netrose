namespace GameMeanMachine.Unity.NetRose
{
    namespace Types
    {
        namespace Models
        {
            /// <summary>
            ///   Used to distinguish objects that are owned -or
            ///   not- by the current client connection. Objects
            ///   not implementing this interface should be
            ///   considered as not being owned by this connection.
            /// </summary>
            public interface IClientOwned
            {
                /// <summary>
                ///   Tells whether the object is owned by the
                ///   current connection or not.
                /// </summary>
                public bool IsOwned();
            }
        }
    }
}