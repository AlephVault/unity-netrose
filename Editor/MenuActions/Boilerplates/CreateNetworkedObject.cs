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
            ///   Utility window used to create networked object files.
            ///   It takes the name, the involved data types (they must
            ///   exist beforehand), and whether to use a simple or an
            ///   owned model as its parent.
            /// </summary>
            public static class CreateNetworkedObject
            {
                /// <summary>
                ///   Utility window used to create the files for a new
                ///   networked object (a pair of behaviours).
                /// </summary>
                public class CreateNetworkedObjectWindow : SmartEditorWindow
                {
                    private Regex nameCriterion = new Regex("^[A-Z][A-Za-z0-9_]*$");
                    private Regex existingNameCriterion = new Regex("^([A-Za-z][A-Za-z0-9_]*\\.)*[A-Za-z][A-Za-z0-9_]*$");
                    
                    // The base name to use.
                    private string baseName = "MyModel";
                    
                    // The name of the SpawnData type.
                    private string spawnDataType = "SpawnData";
                    
                    // The name of the RefreshData type.
                    private string refreshDataType = "RefreshData";

                    // Whether to use OwnedNetRoseModel*Side or not (this
                    // implies that, when this flag is false, the generated
                    // classes have NetRoseModel*Side instead).
                    private bool useOwnedBaseTypes;

                    protected override float GetSmartWidth()
                    {
                        return 750;
                    }

                    protected override void OnAdjustedGUI()
                    {
                        GUIStyle longLabelStyle = MenuActionUtils.GetSingleLabelStyle();

                        EditorGUILayout.LabelField(@"
This utility generates the two networked object files, with boilerplate code and instructions on how to understand that code.

The base name has to be chosen (carefully and according to the game design):
- It must start with an uppercase letter.
- It must continue with letters, numbers, and/or underscores.

The Spawn and Refresh data types must be already existing types, both implementing ISerializable. They can:
- Be simple or fully-qualified names.
- Start with upper or lowercase letters, and continue with letters, numbers and/or underscores.

The two files will be generated:
- {base name}ClientSide to define the networked object client side.
- {base name}ServerSide to define the networked object server side.

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

                        // The Spawn Data type
                        EditorGUILayout.BeginHorizontal();
                        spawnDataType = EditorGUILayout.TextField("Spawn data type", spawnDataType).Trim();
                        bool validSpawnDataType = existingNameCriterion.IsMatch(spawnDataType);
                        if (!validSpawnDataType)
                        {
                            EditorGUILayout.LabelField("The spawn data type name is invalid!");
                        }
                        EditorGUILayout.EndHorizontal();
                        
                        // The Refresh Data type
                        EditorGUILayout.BeginHorizontal();
                        refreshDataType = EditorGUILayout.TextField("Refresh data type", refreshDataType).Trim();
                        bool validRefreshDataType = existingNameCriterion.IsMatch(refreshDataType);
                        if (!validRefreshDataType)
                        {
                            EditorGUILayout.LabelField("The refresh data type name is invalid!");
                        }
                        EditorGUILayout.EndHorizontal();
                        
                        EditorGUILayout.BeginHorizontal();
                        useOwnedBaseTypes = EditorGUILayout.ToggleLeft("Use Owned ('Principal') model base classes",
                            useOwnedBaseTypes);
                        EditorGUILayout.EndHorizontal();

                        if (validBaseName && validSpawnDataType && validRefreshDataType)
                            SmartButton("Generate", Execute);
                    }

                    private void Execute()
                    {
                        DumpProtocolTemplates(
                            baseName, spawnDataType, refreshDataType, useOwnedBaseTypes
                        );
                    }
                }

                // Performs the full dump of the code.
                private static void DumpProtocolTemplates(
                    string basename, string spawnDataType, string refreshDataType, bool useOwned
                ) {
                    string directory = "Packages/com.alephvault.unity.netrose/" +
                                       "Editor/MenuActions/Boilerplates/Templates";

                    // The network object templates.
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
                ///   Opens a dialog to execute the behaviours creation boilerplate.
                /// </summary>
                [MenuItem("Assets/Create/Aleph Vault/NetRose/Boilerplates/Create Networked Object Behaviours", false, 204)]
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

