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
            ///     - Authoring/Behaviours/Protocols/Models/.
            ///   - Scripts/Server/:
            ///     - Authoring/Behaviours/Protocols/Models/.
            /// </summary>
            public static class ProjectStartup
            {
                [MenuItem("Assets/Create/Net Rose/Boilerplates/Project Startup", false, 11)]
                public static void ExecuteBoilerplate()
                {
                    WindRose.MenuActions.Boilerplates.ProjectStartup.ExecuteBoilerplate();
                    AlephVault.Unity.Meetgard.MenuActions.Boilerplates.ProjectStartup.ExecuteBoilerplate();
                    new Boilerplate()
                        .IntoDirectory("Objects", false)
                        .End()
                        .IntoDirectory("Scripts", false)
                            .IntoDirectory("Client", false)
                                .IntoDirectory("Authoring", false)
                                    .IntoDirectory("Behaviours", false)
                                        .IntoDirectory("Protocols", false)
                                            .IntoDirectory("Models")
                                            .End()
                                        .End()
                                    .End()
                                .End()
                            .End()
                            .IntoDirectory("Server", false)
                                .IntoDirectory("Authoring", false)
                                    .IntoDirectory("Behaviours", false)
                                        .IntoDirectory("Protocols", false)
                                            .IntoDirectory("Models")
                                            .End()
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
