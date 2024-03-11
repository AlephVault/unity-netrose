using System.Collections.Generic;
using System.Text.RegularExpressions;
using AlephVault.Unity.Boilerplates.Utils;
using AlephVault.Unity.MenuActions.Types;
using AlephVault.Unity.MenuActions.Utils;
using UnityEditor;
using UnityEngine;

namespace AlephVault.Unity.NetRose
{
    namespace MenuActions
    {
        namespace Boilerplates
        {
            /// <summary>
            ///   Utility window used to create a principal objects protocol.
            ///   It takes the name, the involved network object type basename,
            ///   and generates only a server-side implementation of a principal
            ///   objects protocol (the client-side is always the same).
            /// </summary>
            public static class CreatePrincipalObjectsProtocol
            {
                /// <summary>
                ///   Utility window used to create the file for a new principal
                ///   objects protocol.
                /// </summary>
                public class CreateNetworkedObjectWindow : SmartEditorWindow
                {
                    private Regex nameCriterion = new Regex("^[A-Z][A-Za-z0-9_]*$");
                    private Regex existingNameCriterion = new Regex("^([A-Za-z][A-Za-z0-9_]*\\.)*[A-Za-z][A-Za-z0-9_]*$");
                    
                    // The base name to use.
                    private string baseName = "MyPrincipal";
                    
                    // The base name of the network object type to refer.
                    private string networkObjectTypeBaseName = "MyModel";

                    protected override float GetSmartWidth()
                    {
                        return 750;
                    }

                    protected override void OnAdjustedGUI()
                    {
                        GUIStyle longLabelStyle = MenuActionUtils.GetSingleLabelStyle();

                        EditorGUILayout.LabelField(@"
This utility generates the server side of a principal objects protocol, with boilerplate code and instructions on how to understand that code.

The base name has to be chosen (carefully and according to the game design):
- It must start with an uppercase letter.
- It must continue with letters, numbers, and/or underscores.

The base name for the involved network object behaviour(s) type must belong to already existing types, has to be chosen and can:
- Be simple or fully-qualified names.
- Start with upper or lowercase letters, and continue with letters, numbers and/or underscores.

One single file will be generated:
- {base name}ProtocolServerSide to define the custom new principal objects protocol server side.

WARNING: THIS MIGHT OVERRIDE EXISTING CODE. Always use proper source code management & versioning.
".Trim(), longLabelStyle);

                        // The base name
                        EditorGUILayout.BeginHorizontal();
                        baseName = EditorGUILayout.TextField("Base name", baseName).Trim();
                        bool validBaseName = nameCriterion.IsMatch(baseName);
                        if (!validBaseName)
                        {
                            EditorGUILayout.LabelField("The base name is invalid!");
                        }
                        EditorGUILayout.EndHorizontal();

                        // The network object base name
                        EditorGUILayout.BeginHorizontal();
                        networkObjectTypeBaseName = EditorGUILayout.TextField("Network object", networkObjectTypeBaseName).Trim();
                        bool validNetworkObject = existingNameCriterion.IsMatch(networkObjectTypeBaseName);
                        if (!validNetworkObject)
                        {
                            EditorGUILayout.LabelField("The network object base name is invalid!");
                        }
                        EditorGUILayout.EndHorizontal();

                        if (validBaseName && validNetworkObject)
                            SmartButton("Generate", Execute);
                    }

                    private void Execute()
                    {
                        DumpProtocolTemplates(baseName, networkObjectTypeBaseName);
                    }
                }

                // Performs the full dump of the code.
                private static void DumpProtocolTemplates(
                    string basename, string networkObjectTypeBaseName
                ) {
                    string directory = "Packages/com.alephvault.unity.netrose/" +
                                       "Editor/MenuActions/Boilerplates/Templates";

                    // The protocol templates.
                    TextAsset popss = AssetDatabase.LoadAssetAtPath<TextAsset>(
                        directory + "/PrincipalObjectsNetRoseProtocolServerSide.cs.txt"
                    );
                    
                    Dictionary<string, string> replacements = new Dictionary<string, string>
                    {
                        {"NETWORK_OBJECT", networkObjectTypeBaseName},
                    };

                    new Boilerplate()
                        .IntoDirectory("Scripts", false)
                            .IntoDirectory("Server", false)
                                .IntoDirectory("Authoring", false)
                                    .IntoDirectory("Behaviours", false)
                                        .IntoDirectory("Protocols", false)
                                            .Do(Boilerplate.InstantiateScriptCodeTemplate(
                                                popss, basename + "ProtocolServerSide", replacements
                                            ))
                                        .End()
                                    .End()
                                .End()
                            .End()
                        .End();
                }

                
                /// <summary>
                ///   Opens a dialog to execute the protocol creation boilerplate.
                /// </summary>
                [MenuItem("Assets/Create/Net Rose/Boilerplates/Create Principal Objects Protocol", false, 204)]
                public static void ExecuteBoilerplate()
                {
                    CreateNetworkedObjectWindow window = ScriptableObject.CreateInstance<CreateNetworkedObjectWindow>();
                    window.titleContent = new GUIContent("Networked Object Behaviours generation");
                    window.ShowUtility();
                }
            }
        }
    }
}

