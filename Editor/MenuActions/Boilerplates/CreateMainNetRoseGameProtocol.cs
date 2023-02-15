using System.Collections.Generic;
using System.Text.RegularExpressions;
using AlephVault.Unity.Boilerplates.Utils;
using AlephVault.Unity.MenuActions.Utils;
using UnityEditor;
using UnityEngine;

namespace GameMeanMachine.Unity.NetRose
{
    namespace MenuActions
    {
        namespace Boilerplates
        {
            /// <summary>
            ///   Utility window used to create a simple RPG protocol.
            ///   This protocol has the following features:
            ///   - Has a client-side depending on NetRoseProtocolClientSide.
            ///   - Has a server-side depending on a given child of the
            ///     PrincipalObjectsNetRoseProtocolServerSide{T}.
            ///   - Has a protocol.
            ///   - Hints for the use of throttlers (both in client and
            ///     server side).
            /// </summary>
            public static class CreateMainNetRoseGameProtocol
            {
                /// <summary>
                ///   Utility window used to create the files for the new
                ///   game protocol.
                /// </summary>
                public class CreateMainGameProtocolWindow : EditorWindow
                {
                    private Regex nameCriterion = new Regex("^[A-Z][A-Za-z0-9_]*$");
                    private Regex existingNameCriterion = new Regex("^([A-Za-z][A-Za-z0-9_]*\\.)*[A-Za-z][A-Za-z0-9_]*$");

                    // The base name to use.
                    private string principalProtocolBaseName = "MyPrincipal";
                    
                    // The base name to use.
                    private string baseName = "MyGame";
                    
                    // The base name of the network object type to refer.
                    private string aimType = "AimType";
                    
                    private void OnGUI()
                    {
                        GUIStyle longLabelStyle = MenuActionUtils.GetSingleLabelStyle();

                        EditorGUILayout.BeginVertical();
                        
                        EditorGUILayout.LabelField(@"
This utility generates a game protocol (related to a principal objects one) and a type for the aim data.

The base name has to be chosen (carefully and according to the game design):
- It must start with an uppercase letter.
- It must continue with letters, numbers, and/or underscores.
The same applies for the aim data type (it will also be created: the type that holds data for commands that target a map & position).

The base name for the principal objects protocol type must belong to already existing types, has to be chosen and can:
- Be simple or fully-qualified names.
- Start with upper or lowercase letters, and continue with letters, numbers and/or underscores.

The following files will be generated:
- {base name}ProtocolDefinition to define the custom new game protocol definition.
- {base name}ProtocolClientSide to define the custom new game protocol client side.
- {base name}ProtocolServerSide to define the custom new game protocol server side.
  it will depend on {principal protocol base name}ProtocolServerSide.
- {aim type} to define the custom aim type.

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

                        // The base name
                        EditorGUILayout.BeginHorizontal();
                        aimType = EditorGUILayout.TextField("Aim type", aimType).Trim();
                        bool validAimType = nameCriterion.IsMatch(aimType);
                        if (!validAimType)
                        {
                            EditorGUILayout.LabelField("The aim type is invalid!");
                        }
                        EditorGUILayout.EndHorizontal();

                        // The principal protocol
                        EditorGUILayout.BeginHorizontal();
                        principalProtocolBaseName = EditorGUILayout.TextField("Network object", principalProtocolBaseName).Trim();
                        bool validPrincipalProtocol = existingNameCriterion.IsMatch(principalProtocolBaseName);
                        if (!validPrincipalProtocol)
                        {
                            EditorGUILayout.LabelField("The principal protocol base name is invalid!");
                        }
                        EditorGUILayout.EndHorizontal();
                        
                        bool execute = validBaseName && validPrincipalProtocol && GUILayout.Button("Generate");
                        EditorGUILayout.EndVertical();
                        
                        if (execute) Execute();
                    }

                    private void Execute()
                    {
                        DumpProtocolTemplates(baseName, aimType, principalProtocolBaseName);
                        Close();
                    }
                }

                // Performs the full dump of the code.
                private static void DumpProtocolTemplates(
                    string basename, string aimType, string principalProtocolBaseName
                ) {
                    string directory = "Packages/com.gamemeanmachine.unity.netrose/" +
                                       "Editor/MenuActions/Boilerplates/Templates";

                    // The protocol templates.
                    TextAsset mgpss = AssetDatabase.LoadAssetAtPath<TextAsset>(
                        directory + "/MainGameProtocolServerSide.cs.txt"
                    );
                    TextAsset mgpcs = AssetDatabase.LoadAssetAtPath<TextAsset>(
                        directory + "/MainGameProtocolClientSide.cs.txt"
                    );
                    TextAsset mgdef = AssetDatabase.LoadAssetAtPath<TextAsset>(
                        directory + "/MainGameProtocolDefinition.cs.txt"
                    );
                    TextAsset aim = AssetDatabase.LoadAssetAtPath<TextAsset>(
                        directory + "/AimType.cs.txt"
                    );
                    
                    Dictionary<string, string> replacements = new Dictionary<string, string>
                    {
                        {"AIM", aimType},
                        {"PROTOCOLDEFINITION", basename + "ProtocolDefinition"},
                        {"PRINCIPALPROTOCOL", principalProtocolBaseName + "ProtocolServerSide"}
                    };

                    new Boilerplate()
                        .IntoDirectory("Scripts", false)
                            .IntoDirectory("Server", false)
                                .IntoDirectory("Authoring", false)
                                    .IntoDirectory("Behaviours", false)
                                        .IntoDirectory("Protocols", false)
                                            .Do(Boilerplate.InstantiateScriptCodeTemplate(
                                                mgpss, basename + "ProtocolServerSide", replacements
                                            ))
                                        .End()
                                    .End()
                                .End()
                            .End()
                            .IntoDirectory("Protocols", false)
                                .Do(Boilerplate.InstantiateScriptCodeTemplate(
                                    mgdef, basename + "ProtocolDefinition", replacements
                                ))
                                .IntoDirectory("Messages", false)
                                    .Do(Boilerplate.InstantiateScriptCodeTemplate(
                                        aim, aimType, replacements
                                    ))
                                .End()
                            .End()
                            .IntoDirectory("Client", false)
                                .IntoDirectory("Authoring", false)
                                    .IntoDirectory("Behaviours", false)
                                        .IntoDirectory("Protocols", false)
                                            .Do(Boilerplate.InstantiateScriptCodeTemplate(
                                                mgpcs, basename + "ProtocolClientSide", replacements
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
                [MenuItem("Assets/Create/Net Rose/Boilerplates/Create Main Game Protocol", false, 205)]
                public static void ExecuteBoilerplate()
                {
                    CreateMainGameProtocolWindow window = ScriptableObject.CreateInstance<CreateMainGameProtocolWindow>();
                    Vector2 size = new Vector2(750, 372);
                    window.position = new Rect(new Vector2(110, 250), size);
                    window.minSize = size;
                    window.maxSize = size;
                    window.titleContent = new GUIContent("Main game protocol generation");
                    window.ShowUtility();
                }
            }
        }
    }
}
