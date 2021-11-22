using UnityEngine;
using AlephVault.Unity.Meetgard.Scopes.Authoring.Behaviours.Client;
using AlephVault.Unity.Meetgard.Types;
using GameMeanMachine.Unity.NetRose.Samples.Common.Types;
using GameMeanMachine.Unity.NetRose.Authoring.Behaviours.Client;


namespace GameMeanMachine.Unity.NetRose
{
    namespace Samples
    {
        namespace Client
        {
            public class OwnableModelClientSide : NetRoseModelClientSide<Ownable, Ownable>
            {
                public bool IsOwned { get; private set; }

                protected override void InflateFrom(Ownable fullData)
                {
                    IsOwned = fullData.IsOwned;
                    Update();
                }

                protected override void UpdateFrom(Ownable refreshData)
                {
                    IsOwned = refreshData.IsOwned;
                    Update();
                }

                private void Update()
                {
                    if (IsOwned && Camera.main) Camera.main.transform.position = new Vector3(
                        transform.position.x, transform.position.y, -10
                    );
                }
            }
        }
    }
}
