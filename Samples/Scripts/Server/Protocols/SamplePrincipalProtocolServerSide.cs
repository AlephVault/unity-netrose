using System;
using System.Threading.Tasks;
using UnityEngine;
using AlephVault.Unity.NetRose.Authoring.Behaviours.Server;
using AlephVault.Unity.WindRose.Authoring.Behaviours.World;


namespace AlephVault.Unity.NetRose
{
    namespace Samples
    {
        namespace Server
        {
            namespace Protocols
            {
                public class SamplePrincipalProtocolServerSide : PrincipalObjectsNetRoseProtocolServerSide<OwnableModelServerSide>
                {
                    [SerializeField]
                    private int char1index = 1;

                    [SerializeField]
                    private int char2index = 2;
                    
                    public override async Task OnConnected(ulong clientId)
                    {
                        Debug.Log($"ClientMovementProtocolServerSide.OnConnected({clientId})::Queue");
                        try
                        {
                            InstantiatePrincipal(
                                clientId, clientId % 2 == 1 ? char1index : char2index,
                                () => ScopesProtocolServerSide.LoadedScopes[4].GetComponent<Scope>()[0],
                                8, 6, (obj) =>
                                {
                                    obj.LastCommandTime = 0;
                                }
                            );
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }
                    }

                    public override async Task OnDisconnected(ulong clientId, System.Exception reason)
                    {
                        Debug.Log($"ClientMovementProtocolServerSide.OnDisconnected({clientId})::Queue");
                        RemovePrincipal(clientId);
                    }
                }
            }
        }
    }
}