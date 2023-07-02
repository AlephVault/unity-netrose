using System;
using UnityEngine;
using AlephVault.Unity.Meetgard.Scopes.Authoring.Behaviours.Client;
using AlephVault.Unity.Meetgard.Types;
using AlephVault.Unity.NetRose.Samples.Common.Types;
using AlephVault.Unity.NetRose.Authoring.Behaviours.Client;
using AlephVault.Unity.NetRose.Authoring.Behaviours.Server;
using AlephVault.Unity.NetRose.Types.Models;


namespace AlephVault.Unity.NetRose
{
    namespace Samples
    {
        namespace Client
        {
            public class OwnableModelClientSide : OwnedNetRoseModelClientSide<Nothing, Nothing>
            {
                private static Camera camera;
                
                protected override void InflateOwnedFrom(Nothing fullData)
                {
                    if (IsOwned()) camera = Camera.main;
                }
                
                protected override void UpdateFrom(Nothing refreshData)
                {
                }

                private void Update()
                {
                    if (IsOwned())
                    {
                        camera.transform.position = new Vector3(
                            transform.position.x, transform.position.y,
                            transform.position.z - 10
                        );
                    }
                }
            }
        }
    }
}
