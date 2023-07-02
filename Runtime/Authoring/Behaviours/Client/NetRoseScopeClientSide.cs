using AlephVault.Unity.Meetgard.Scopes.Authoring.Behaviours.Client;
using AlephVault.Unity.WindRose.Authoring.Behaviours.World;
using UnityEngine;


namespace AlephVault.Unity.NetRose
{
    namespace Authoring
    {
        namespace Behaviours
        {
            namespace Client
            {
                /// <summary>
                ///   The scope client side of a netrose scope knows how
                ///   to reach the related client scope and also all the
                ///   involved WindRose maps.
                /// </summary>
                [RequireComponent(typeof(ScopeClientSide))]
                [RequireComponent(typeof(Scope))]
                public class NetRoseScopeClientSide : MonoBehaviour
                {
                    /// <summary>
                    ///   The related client server side.
                    /// </summary>
                    public ScopeClientSide ScopeClientSide { get; private set; }

                    /// <summary>
                    ///   The related world scope (to get the maps).
                    /// </summary>
                    public Scope Maps { get; private set; }

                    private void Awake()
                    {
                        ScopeClientSide = GetComponent<ScopeClientSide>();
                        ScopeClientSide.OnLoad += ScopeClientSide_OnLoad;
                        Maps = GetComponent<Scope>();
                    }

                    private void ScopeClientSide_OnLoad()
                    {
                        Maps.Initialize();
                    }

                    private void OnDestroy()
                    {
                        ScopeClientSide.OnLoad -= ScopeClientSide_OnLoad;
                    }

                    /// <summary>
                    ///   The scope client side id.
                    /// </summary>
                    public uint Id { get { return ScopeClientSide.Id; } }
                }
            }
        }
    }
}
