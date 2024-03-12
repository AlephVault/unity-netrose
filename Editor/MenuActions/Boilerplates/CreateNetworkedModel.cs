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
            ///   Utility window used to create a serializable type file.
            ///   It takes the name only, and creates a type stub in the
            ///   Models directory.
            /// </summary>
            public static class CreateNetworkedModel
            {
                /// <summary>
                ///   Utility window used to create the files for a new
                ///   networked object (a pair of behaviours).
                /// </summary>
                public class CreateNetworkedModelWindow : SmartEditorWindow
                {
                    private Regex nameCriterion = new Regex("^[A-Z][A-Za-z0-9_]*$");
                    
                    // The base name to use.
                    private string baseName = "MyType";

                    protected override float GetSmartWidth()
                    {
                        return 750;
                    }

                    protected override void OnAdjustedGUI()
                    {
                        GUIStyle longLabelStyle = MenuActionUtils.GetSingleLabelStyle();

                        EditorGUILayout.LabelField(@"
This utility generates the serializable type file, with boilerplate code and instructions on how to understand that code.

The base name has to be chosen (carefully and according to the game design):
- It must start with an uppercase letter.
- It must continue with letters, numbers, and/or underscores.

The file will be generated:
- {base name} to define the type file.

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

                        if (validBaseName) SmartButton("Generate", Execute);
                    }

                    private void Execute()
                    {
                        DumpTypeTemplates(baseName);
                    }
                }

                // Performs the full dump of the code.
                private static void DumpTypeTemplates(string basename) {
                    string directory = "Packages/com.alephvault.unity.netrose/" +
                                       "Editor/MenuActions/Boilerplates/Templates";

                    // The network object templates.
                    TextAsset mt = AssetDatabase.LoadAssetAtPath<TextAsset>(
                        directory + "/SerializableType.cs.txt"
                    );
                    
                    Dictionary<string, string> replacements = new Dictionary<string, string>();

                    new Boilerplate()
                        .IntoDirectory("Scripts", false)
                            .IntoDirectory("Models", false)
                                .Do(Boilerplate.InstantiateScriptCodeTemplate(
                                    mt, basename, replacements
                                ))
                            .End()
                        .End();
                }

                
                /// <summary>
                ///   Opens a dialog to execute the behaviours creation boilerplate.
                /// </summary>
                [MenuItem("Assets/Create/AlephVault/Net Rose/Boilerplates/Create Serializable Type", false, 203)]
                public static void ExecuteBoilerplate()
                {
                    CreateNetworkedModelWindow window = ScriptableObject.CreateInstance<CreateNetworkedModelWindow>();
                    window.titleContent = new GUIContent("Serializable type generation");
                    window.ShowUtility();
                }
            }
        }
    }
}
