using System;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace GameMeanMachine.Unity.NetRose
{
    namespace MenuActions
    {
        namespace Objects
        {
            public static class ConvertToNetworkedScopes
            {
                [MenuItem("Assets/Create/Net Rose/Objects/Networked Object Scopes (From 1+ selected core object scopes)", false, priority = 203)]
                public static void ExecuteWrapper()
                {
                    Type invalid = Type.GetType(
                        "GameMeanMachine.Unity.NetRose.Authoring.Behaviours.Server.NetRoseScopeServerSide, Pija"
                    ); 
                    Debug.Log($"Type: {(invalid == null ? "null" : invalid.FullName)}");
                    Type tortor = Type.GetType(
                        "Tortor, Assembly-CSharp"
                    );
                    Debug.Log($"Type: {(tortor == null ? "null" : tortor.FullName)}");
                    Debug.Log($"Type: {System.Type.GetType("Client.Authoring.Behaviours.NetworkObjects.MyBehaviourClientSide, Assembly-CSharp")}");
                    Debug.Log($"Type: {System.Type.GetType("Server.Authoring.Behaviours.NetworkObjects.MyBehaviourServerSide, Assembly-CSharp")}");
                    Debug.Log($"Type: {System.Type.GetType("GameMeanMachine.Unity.NetRose.Authoring.Behaviours.Server.NetRoseScopeServerSide, GameMeanMachine.Unity.NetRose.Runtime")}");
                    // foreach(string guid in AssetDatabase.FindAssets("t:script"))
                    // {
                    //     Debug.Log("path: " + AssetDatabase.GUIDToAssetPath(guid));
                    // };
                }
            }
        }
    }
}
