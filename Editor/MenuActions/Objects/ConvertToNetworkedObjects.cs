using System;
using System.Linq;
using System.Text.RegularExpressions;
using AlephVault.Unity.MenuActions.Types;
using AlephVault.Unity.MenuActions.Utils;
using AlephVault.Unity.WindRose.Authoring.Behaviours.Entities.Objects;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AlephVault.Unity.NetRose
{
    namespace MenuActions
    {
        namespace Objects
        {
            public static class ConvertToNetworkedObjects
            {
                private const string PREFIXDIR_CORE = "Assets/Objects/Prefabs/Core/Objects/";
                private const string PREFIXDIR_CLIENT = "Assets/Objects/Prefabs/Client/Objects/";
                private const string PREFIXDIR_SERVER = "Assets/Objects/Prefabs/Server/Objects/";

                private const string PREFIXNAMESPACE_CLIENT = "Client.Authoring.Behaviours.NetworkObjects.";
                private const string PREFIXNAMESPACE_SERVER = "Server.Authoring.Behaviours.NetworkObjects.";
                private const string SUFFIXTYPE_CLIENT = "ClientSide, Assembly-CSharp";
                private const string SUFFIXTYPE_SERVER = "ServerSide, Assembly-CSharp";

                /// <summary>
                ///   Utility window used to create networked prefabs
                ///   out of a selected base prefab.
                /// </summary>
                public class ConvertToNetworkedObjectsWindow : SmartEditorWindow
                {
                    private Regex existingNameCriterion = new Regex("^([A-Za-z][A-Za-z0-9_]*\\.)*[A-Z][A-Za-z0-9_]*$");
                    
                    // The base name to use.
                    private string baseName = "MyModel";

                    protected override float GetSmartWidth()
                    {
                        return 750;
                    }

                    protected override void OnAdjustedGUI()
                    {
                        GUIStyle longLabelStyle = MenuActionUtils.GetSingleLabelStyle();

                        EditorGUILayout.LabelField($@"
This utility generates the two networked prefabs, one for the client and one for the server, from a selected MapObject prefab (in the core objects directory).

A base name of an existing pair of types must be selected. The base name may include a sub-namespace, so these types need to already exist:
- {PREFIXDIR_SERVER}{{base name}}ServerSide (somewhere deep in the Assets/... directory).
- {PREFIXDIR_CLIENT}{{base name}}ClientSide (somewhere deep in the Assets/... directory).

One or more prefabs can be selected but, in order to not be filtered out, they must satisfy:
- To be located into {PREFIXDIR_CORE}, or a subdirectory of it.
- To have a behaviour attached in their root, of type (WindRose's) MapObject.

Two prefabs will be generated but, in order to be successful, this must occur for each prefab:
- The directory {PREFIXDIR_CLIENT} must exist to generate the client one, and {PREFIXDIR_SERVER} for the server one.
- Any subdirectory path the prefab is located into must also exist in the corresponding server/client directory as well.

WARNING: THIS MIGHT OVERRIDE EXISTING ASSETS. Always use proper source code management & versioning.
".Trim(), longLabelStyle);

                        EditorGUILayout.BeginHorizontal();
                        baseName = EditorGUILayout.TextField("Base name", baseName);
                        bool validBaseName = existingNameCriterion.IsMatch(baseName);
                        if (!validBaseName)
                        {
                            EditorGUILayout.LabelField("The base name is invalid");
                        }
                        EditorGUILayout.EndHorizontal();

                        if (validBaseName)
                            SmartButton("Make Prefab(s)", () => { GenerateNetworkedPrefabs(baseName); });
                    }
                }

                // Given a network object type basename, generates new prefabs for
                // each selected prefab.
                private static void GenerateNetworkedPrefabs(string baseName)
                {
                    Type serverSideType = Type.GetType(
                        PREFIXNAMESPACE_SERVER + baseName + SUFFIXTYPE_SERVER
                    );
                    if (serverSideType == null)
                    {
                        throw new ArgumentException(
                            $"The type {PREFIXNAMESPACE_SERVER + baseName}ServerSide could not be found"
                        );
                    }

                    Type clientSideType = Type.GetType(
                        PREFIXNAMESPACE_CLIENT + baseName + SUFFIXTYPE_CLIENT
                    );
                    if (clientSideType == null)
                    {
                        throw new ArgumentException(
                            $"The type {PREFIXNAMESPACE_CLIENT + baseName}ClientSide could not be found"
                        );
                    }
                    
                    foreach (var obj in Selection.GetFiltered<MapObject>(SelectionMode.Assets))
                    {
                        string path = AssetDatabase.GetAssetPath(obj);
                        if (path.StartsWith(PREFIXDIR_CORE))
                        {
                            string subPath = path.Substring(PREFIXDIR_CORE.Length);
                            
                            string clientPath = PREFIXDIR_CLIENT + subPath;
                            GameObject clientObj = (GameObject)PrefabUtility.InstantiatePrefab(obj.gameObject);
                            clientObj.AddComponent(clientSideType);
                            PrefabUtility.SaveAsPrefabAsset(clientObj.gameObject, clientPath);
                            Object.DestroyImmediate(clientObj);

                            string serverPath = PREFIXDIR_SERVER + subPath;
                            GameObject serverObj = (GameObject)PrefabUtility.InstantiatePrefab(obj.gameObject);
                            serverObj.AddComponent(serverSideType);
                            PrefabUtility.SaveAsPrefabAsset(serverObj.gameObject, serverPath);
                            Object.DestroyImmediate(serverObj);
                        }
                    }
                }
                
                [MenuItem("Assets/Create/Aleph Vault/NetRose/Objects/Networked Object Prefabs (From 1+ selected core object prefabs)", false, priority = 203)]
                public static void ExecuteWrapper()
                {
                    ConvertToNetworkedObjectsWindow window = ScriptableObject.CreateInstance<ConvertToNetworkedObjectsWindow>();
                    window.titleContent = new GUIContent("Networked Objects conversion");
                    window.ShowUtility();
                }

                [MenuItem("Assets/Create/Aleph Vault/NetRose/Objects/Networked Object Prefabs (From 1+ selected core object prefabs)", true)]
                public static bool CanExecuteWrapper()
                {
                    return (from obj in Selection.GetFiltered<MapObject>(SelectionMode.Assets)
                            where AssetDatabase.GetAssetPath(obj).StartsWith(PREFIXDIR_CORE)
                            select obj).Any();
                }
            }
        }
    }
}
