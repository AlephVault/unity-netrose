using AlephVault.Unity.Meetgard.Protocols;


namespace GameMeanMachine.Unity.NetRose
{
    namespace Samples
    {
        namespace Common
        {
            namespace Protocols
            {
                public class ClientMovementProtocolDefinition : ProtocolDefinition
                {
                    protected override void DefineMessages()
                    {
                        DefineClientMessage("Move:Down");
                        DefineClientMessage("Move:Left");
                        DefineClientMessage("Move:Right");
                        DefineClientMessage("Move:Up");
                    }
                }
            }
        }
    }
}