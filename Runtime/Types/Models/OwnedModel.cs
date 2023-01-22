using AlephVault.Unity.Binary;


namespace GameMeanMachine.Unity.NetRose
{
    namespace Types
    {
        namespace Models
        {
            /// <summary>
            ///   Owned models wrap the inner data to also tell
            ///   whether the current client the object is linked
            ///   to is the owner or not.
            /// </summary>
            public class OwnedModel<ModelData> : ISerializable
                where ModelData : class, ISerializable, new()
            {
                /// <summary>
                ///   Whether this object must be understood as
                ///   owned (for the clients) or not.
                /// </summary>
                public bool Owned;
                
                /// <summary>
                ///   The current object data.
                /// </summary>
                public ModelData Data;
                
                public void Serialize(Serializer serializer)
                {
                    serializer.Serialize(ref Owned);
                    Data ??= new ModelData();
                    Data.Serialize(serializer);
                }

                /// <summary>
                ///   Creates an owned model out of a simple model.
                /// </summary>
                /// <param name="data">The data to wrap</param>
                /// <returns>An owned model with the data (marked as owned)</returns>
                public static OwnedModel<ModelData> AsOwned(ModelData data)
                {
                    return new OwnedModel<ModelData>() {Owned = true, Data = data};
                }
                
                /// <summary>
                ///   Creates an owned model out of a simple model.
                /// </summary>
                /// <param name="data">The data to wrap</param>
                /// <returns>An owned model with the data (marked as not owned)</returns>
                public static OwnedModel<ModelData> AsNotOwned(ModelData data)
                {
                    return new OwnedModel<ModelData>() {Owned = false, Data = data};
                }
            }
        }
    }
}
