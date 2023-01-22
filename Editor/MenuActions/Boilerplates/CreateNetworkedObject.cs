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
            ///   Utility window used to create networked object files.
            ///   It takes the name, the involved data types (they must
            ///   exist beforehand), and whether to use a simple or an
            ///   owned model as its parent.
            /// </summary>
            public static class CreateNetworkedObject
            {
                // Performs the full dump of the code.
                private static void DumpProtocolTemplates(
                    string basename, string spawnDataType, string refreshDataType, bool useOwned
                ) {
                    string directory = "Packages/com.gamemeanmachine.unity.netrose/" +
                                       "Editor/MenuActions/Boilerplates/Templates";

                    // The protocol templates.
                    TextAsset mcs = AssetDatabase.LoadAssetAtPath<TextAsset>(
                        directory + (useOwned ? "/OwnedNetRoseModelClientSide.cs.txt" : "/NetRoseModelClientSide.cs.txt")
                    );
                    TextAsset mss = AssetDatabase.LoadAssetAtPath<TextAsset>(
                        directory + (useOwned ? "/OwnedNetRoseModelServerSide.cs.txt" : "/NetRoseModelServerSide.cs.txt")
                    );
                    
                    Dictionary<string, string> replacements = new Dictionary<string, string>
                    {
                        {"SPAWNDATA_TYPE", spawnDataType},
                        {"REFRESHDATA_TYPE", refreshDataType}
                    };

                    new Boilerplate()
                        .IntoDirectory("Objects", false)
                        .End()
                        .IntoDirectory("Scripts", false)
                            .IntoDirectory("Client", false)
                                .IntoDirectory("Authoring", false)
                                    .IntoDirectory("Behaviours", false)
                                        .IntoDirectory("NetworkObjects", false)
                                            .Do(Boilerplate.InstantiateScriptCodeTemplate(
                                                mcs, basename + "ClientSide", replacements
                                            ))
                                        .End()
                                    .End()
                                .End()
                            .End()
                            .IntoDirectory("Server", false)
                                .IntoDirectory("Authoring", false)
                                    .IntoDirectory("Behaviours", false)
                                        .IntoDirectory("NetworkObjects", false)
                                            .Do(Boilerplate.InstantiateScriptCodeTemplate(
                                                mss, basename + "ServerSide", replacements
                                            ))
                                        .End()
                                    .End()
                                .End()
                            .End()
                        .End();
                }

                
                /// <summary>
                ///   Opens a dialog to execute the strategy creation boilerplate.
                /// </summary>
                [MenuItem("Assets/Create/Net Rose/Boilerplates/Networked Object Behaviours", false, 13)]
                public static void ExecuteBoilerplate()
                {
                    DumpProtocolTemplates("PlayerCharacter", "String", "String", false);
                    DumpProtocolTemplates("NPC", "String", "String", true);
                    // CreateAuthProtocolWindow window = ScriptableObject.CreateInstance<CreateAuthProtocolWindow>();
                    // Vector2 size = new Vector2(750, 394);
                    // window.position = new Rect(new Vector2(110, 250), size);
                    // window.minSize = size;
                    // window.maxSize = size;
                    // window.titleContent = new GUIContent("Networked Object Behaviours generation");
                    // window.ShowUtility();
                }
            }
        }
    }
}

