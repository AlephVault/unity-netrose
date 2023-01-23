using System.Collections;
using System.Collections.Generic;
using AlephVault.Unity.Boilerplates.Utils;
using UnityEditor;
using UnityEngine;

namespace GameMeanMachine.Unity.NetRose
{
    namespace MenuActions
    {
        namespace Boilerplates
        {
            /// <summary>
            ///   This boilerplate function creates:
            ///   - Complete WindRose layout.
            ///   - Complete Meetgard layout.
            ///   - Objects/Prefabs/:
            ///     - Client/: Objects/, Scopes/.
            ///     - Server/: Objects/, Scopes/.
            ///   - Scripts/Client/:
            ///     - Authoring/Behaviours/NetworkObjects/.
            ///   - Scripts/Server/:
            ///     - Authoring/Behaviours/NetworkObjects/.
            /// </summary>
            public static class ProjectStartup
            {
                [MenuItem("Assets/Create/Net Rose/Boilerplates/Project Startup", false, 13)]
                public static void ExecuteBoilerplate()
                {
                    WindRose.MenuActions.Boilerplates.ProjectStartup.ExecuteBoilerplate();
                    AlephVault.Unity.Meetgard.MenuActions.Boilerplates.ProjectStartup.ExecuteBoilerplate();
                    new Boilerplate()
                        .IntoDirectory("Scripts", false)
                            .IntoDirectory("Client", false)
                                .IntoDirectory("Authoring", false)
                                    .IntoDirectory("Behaviours", false)
                                        .IntoDirectory("NetworkObjects")
                                        .End()
                                    .End()
                                .End()
                            .End()
                            .IntoDirectory("Models")
                            .End()
                            .IntoDirectory("Server", false)
                                .IntoDirectory("Authoring", false)
                                    .IntoDirectory("Behaviours", false)
                                        .IntoDirectory("NetworkObjects")
                                        .End()
                                    .End()
                                .End()
                            .End()
                        .End();
                }
            }
        }
    }
}
