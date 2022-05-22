using AlephVault.Unity.Meetgard.Scopes.Authoring.Behaviours.Server;
using GameMeanMachine.Unity.NetRose.Authoring.Behaviours.Server;
using GameMeanMachine.Unity.WindRose.Authoring.Behaviours.Entities.Objects;
using GameMeanMachine.Unity.WindRose.Authoring.Behaviours.Entities.Objects.Teleport;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


namespace GameMeanMachine.Unity.NetRose
{
    namespace Samples
    {
        namespace Server
        {
            /// <summary>
            ///   Links the door to a certain target given by its key settings.
            /// </summary>
            [RequireComponent(typeof(Door))]
            [RequireComponent(typeof(EmptyModelServerSide))]
            public class DoorLinker : MonoBehaviour
            {
                private static Dictionary<string, DoorLinker> doorLinkersByName = new Dictionary<string, DoorLinker>();

                private string doorName = "";
                public string TargetName = "";

                private EmptyModelServerSide emptyModelServerSide;
                private SimpleTeleporter _simpleTeleporter;

                private void Awake()
                {
                    emptyModelServerSide = GetComponent<EmptyModelServerSide>();
                    _simpleTeleporter = GetComponent<SimpleTeleporter>();
                    emptyModelServerSide.OnSpawned += EmptyModelServerSide_OnSpawned;
                }

                private async Task EmptyModelServerSide_OnSpawned()
                {
                    try
                    {
                        _simpleTeleporter.Target = doorLinkersByName[TargetName].GetComponent<SimpleTeleportTarget>();
                    }
                    catch (KeyNotFoundException)
                    {
                        Destroy(gameObject);
                        throw new Exception("Invalid door linker name for target: " + TargetName);
                    }
                    catch(Exception e)
                    {
                        Debug.LogException(e);
                    }
                }

                public string DoorName
                {
                    get
                    {
                        return doorName;
                    }
                    set
                    {
                        if (value == "" || value == null)
                        {
                            throw new Exception("Door linkers must have a name");
                        }
                        if (doorLinkersByName.ContainsKey(value))
                        {
                            throw new Exception("Door linker name already in use: " + value);
                        }
                        doorLinkersByName.Remove(doorName);
                        doorName = value;
                        doorLinkersByName.Add(doorName, this);
                    }
                }

                private void OnDestroy()
                {
                    doorLinkersByName.Remove(doorName);
                }
            }
        }
    }
}