using System;
using UnityEngine;
using AlephVault.Unity.Meetgard.Scopes.Authoring.Behaviours.Client;
using AlephVault.Unity.Meetgard.Types;
using GameMeanMachine.Unity.NetRose.Samples.Common.Types;
using GameMeanMachine.Unity.NetRose.Authoring.Behaviours.Client;
using GameMeanMachine.Unity.NetRose.Authoring.Behaviours.Server;
using GameMeanMachine.Unity.NetRose.Types.Models;


namespace GameMeanMachine.Unity.NetRose
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
