using AlephVault.Unity.Layout.Utils;
using AlephVault.Unity.Meetgard.Scopes.Authoring.Behaviours.Server;
using AlephVault.Unity.NetRose.Authoring.Behaviours.Server;
using AlephVault.Unity.WindRose.Authoring.Behaviours.Entities.Objects;
using AlephVault.Unity.WindRose.Authoring.Behaviours.Entities.Objects.Teleport;
using AlephVault.Unity.WindRose.Authoring.Behaviours.World;
using AlephVault.Unity.WindRose.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


namespace AlephVault.Unity.NetRose
{
    namespace Samples
    {
        namespace Server
        {
            [RequireComponent(typeof(NetRoseScopeServerSide))]
            public class TeleportSpawner : MonoBehaviour
            {
                [Serializable]
                public struct TeleportSetting
                {
                    public string TeleportName;
                    public uint MapIdx;
                    public ushort X;
                    public ushort Y;
                    public string TeleportTarget;
                    public bool ForcesDirection;
                    public Direction Direction;
                }

                [SerializeField]
                private TeleportSetting[] teleports;

                private List<DoorLinker> linkers;

                [SerializeField]
                private uint teleportIndex;

                private ScopeServerSide ScopeServerSide;
                private Scope Scope;

                private void Awake()
                {
                    ScopeServerSide = GetComponent<ScopeServerSide>();
                    ScopeServerSide.OnLoad += ScopeServerSide_OnLoad;
                    Scope = GetComponent<Scope>();
                }

                private void OnDestroy()
                {
                    ScopeServerSide.OnLoad -= ScopeServerSide_OnLoad;
                }

                private async Task ScopeServerSide_OnLoad()
                {
                    linkers = new List<DoorLinker>();
                    foreach (TeleportSetting teleport in teleports)
                    {
                        try
                        {
                            ObjectServerSide obj = ScopeServerSide.Protocol.InstantiateHere(teleportIndex);
                            DoorLinker doorLinker = obj.GetComponent<DoorLinker>();
                            SimpleTeleportTarget teleportTarget = obj.GetComponent<SimpleTeleportTarget>();
                            doorLinker.DoorName = teleport.TeleportName;
                            doorLinker.TargetName = teleport.TeleportTarget;
                            linkers.Add(doorLinker);
                            MapObject mapObj = ((INetRoseModelServerSide)obj).MapObject;
                            // Initialize it, to recognize itself as NOT attached beforehand.
                            // Otherwise, when attaching it, the initialization would count
                            // as double attachment.
                            mapObj.Initialize();
                            // And finally, force the orientation of this teleporter.
                            teleportTarget.ForceOrientation = teleport.ForcesDirection;
                            teleportTarget.NewOrientation = teleport.Direction;
                            // var _ = ScopeServerSide.AddObject(obj);
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogException(e);
                        }
                    }
                    Invoke("AttachEverything", 1);
                }

                private void AttachEverything()
                {
                    int index = 0;
                    foreach (TeleportSetting teleport in teleports)
                    {
                        // Then, attach the object.
                        MapObject mapObj = linkers[index].GetComponent<INetRoseModelServerSide>().MapObject;
                        mapObj.Attach(Scope[(int)teleport.MapIdx], teleport.X, teleport.Y, true);
                        index++;
                    }
                }
            }
        }
    }
}
