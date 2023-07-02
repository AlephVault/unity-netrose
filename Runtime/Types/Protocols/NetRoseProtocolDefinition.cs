using AlephVault.Unity.Binary.Wrappers;
using AlephVault.Unity.Meetgard.Protocols;
using AlephVault.Unity.Meetgard.Types;
using AlephVault.Unity.NetRose.Types.Models;
using AlephVault.Unity.NetRose.Types.Protocols.Messages;
using AlephVault.Unity.WindRose.Types;
using System;


namespace AlephVault.Unity.NetRose
{
    namespace Types
    {
        namespace Protocols
        {
            /// <summary>
            ///   This is an abstract class for the NetRose protocol and involved
            ///   subclasses. It defines the standard NetRose messages.
            /// </summary>
            public class NetRoseProtocolDefinition : ProtocolDefinition
            {
                protected override void DefineMessages()
                {
                    DefineServerMessage<ObjectMessage<Attachment>>("Object:Attached");
                    DefineServerMessage<ObjectMessage<Nothing>>("Object:Detached");
                    DefineServerMessage<ObjectMessage<MovementStart>>("Object:Movement:Started");
                    DefineServerMessage<ObjectMessage<Position>>("Object:Movement:Cancelled");
                    DefineServerMessage<ObjectMessage<Position>>("Object:Movement:Rejected");
                    DefineServerMessage<ObjectMessage<Position>>("Object:Movement:Finished");
                    DefineServerMessage<ObjectMessage<Position>>("Object:Teleported");
                    DefineServerMessage<ObjectMessage<UInt>>("Object:Speed:Changed");
                    DefineServerMessage<ObjectMessage<Enum<Direction>>>("Object:Orientation:Changed");
                }
            }
        }
    }
}
