using System.Linq;
using AlephVault.Unity.NetRose.Authoring.Behaviours.Client;
using AlephVault.Unity.NetRose.Authoring.Behaviours.Server;
using AlephVault.Unity.WindRose.Authoring.Behaviours.World;
using UnityEditor;
using UnityEngine;

namespace AlephVault.Unity.NetRose
{
    namespace MenuActions
    {
        namespace Objects
        {
            public static class ConvertToNetworkedScopes
            {
                private const string PREFIXDIR_CORE = "Assets/Objects/Prefabs/Core/Scopes/";
                private const string PREFIXDIR_CLIENT = "Assets/Objects/Prefabs/Client/Scopes/";
                private const string PREFIXDIR_SERVER = "Assets/Objects/Prefabs/Server/Scopes/";
                
                [MenuItem("Assets/Create/Aleph Vault/NetRose/Objects/Networked Object Scopes (From 1+ selected core object scopes)", false, priority = 203)]
                public static void ExecuteWrapper()
                {
                    foreach (var obj in Selection.GetFiltered<Scope>(SelectionMode.Assets))
                    {
                        string path = AssetDatabase.GetAssetPath(obj);
                        if (path.StartsWith(PREFIXDIR_CORE))
                        {
                            string subPath = path.Substring(PREFIXDIR_CORE.Length);
                            
                            string clientPath = PREFIXDIR_CLIENT + subPath;
                            GameObject clientObj = (GameObject)PrefabUtility.InstantiatePrefab(obj.gameObject, null);
                            clientObj.AddComponent<NetRoseScopeClientSide>();
                            PrefabUtility.SaveAsPrefabAsset(clientObj.gameObject, clientPath);
                            Object.DestroyImmediate(clientObj);

                            string serverPath = PREFIXDIR_SERVER + subPath;
                            GameObject serverObj = (GameObject)PrefabUtility.InstantiatePrefab(obj.gameObject, null);
                            serverObj.AddComponent<NetRoseScopeServerSide>();
                            PrefabUtility.SaveAsPrefabAsset(serverObj.gameObject, serverPath);
                            Object.DestroyImmediate(serverObj);
                        }
                    }
                }

                [MenuItem("Assets/Create/Aleph Vault/NetRose/Objects/Networked Object Scopes (From 1+ selected core object scopes)", true)]
                public static bool CanExecuteWrapper()
                {
                    return (from obj in Selection.GetFiltered<Scope>(SelectionMode.Assets)
                        where AssetDatabase.GetAssetPath(obj).StartsWith(PREFIXDIR_CORE)
                        select obj).Any();
                }
            }
        }
    }
}
