using UnityEngine;
using UnityEditor;
using MistInteractive.ThirdPerson.Player;
using System.Linq;

namespace MistInteractive.ThirdPerson.Editor
{
    /// <summary>
    /// Custom inspector for PlayerStateMachine.
    /// Adds a button to auto-detect and assign all PlayerModule assets in the project.
    /// </summary>
    [CustomEditor(typeof(PlayerStateMachine))]
    public class PlayerStateMachineEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);

            var stateMachine = (PlayerStateMachine)target;
            var serializedObj = new SerializedObject(stateMachine);
            var modulesProp = serializedObj.FindProperty("modules");

            // Auto-detect button
            if (GUILayout.Button("Auto-Detect Modules in Project", GUILayout.Height(30)))
            {
                AutoDetectModules(modulesProp);
                serializedObj.ApplyModifiedProperties();
            }

            // Info box
            EditorGUILayout.HelpBox(
                "Auto-Detect will find all PlayerModule assets in your project and add any missing modules to the list. " +
                "It will not remove existing modules or add duplicates.",
                MessageType.Info
            );
        }

        private void AutoDetectModules(SerializedProperty modulesProp)
        {
            // Find all PlayerModule assets in project
            var guids = AssetDatabase.FindAssets($"t:{nameof(PlayerModule)}");
            var foundModules = guids
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => AssetDatabase.LoadAssetAtPath<PlayerModule>(path))
                .Where(module => module != null)
                .ToList();

            if (foundModules.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "No Modules Found",
                    "No PlayerModule assets found in the project. Create at least a LocomotionModule via:\n\n" +
                    "Right-click → Create → MiST → Player Modules → Locomotion",
                    "OK"
                );
                return;
            }

            // Get existing modules to avoid duplicates
            var existingModules = new System.Collections.Generic.HashSet<PlayerModule>();
            for (int i = 0; i < modulesProp.arraySize; i++)
            {
                var existingModule = modulesProp.GetArrayElementAtIndex(i).objectReferenceValue as PlayerModule;
                if (existingModule != null)
                {
                    existingModules.Add(existingModule);
                }
            }

            // Add missing modules
            int addedCount = 0;
            foreach (var module in foundModules)
            {
                if (!existingModules.Contains(module))
                {
                    modulesProp.arraySize++;
                    var newElement = modulesProp.GetArrayElementAtIndex(modulesProp.arraySize - 1);
                    newElement.objectReferenceValue = module;
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                Debug.Log($"[PlayerStateMachine] Auto-detected and added {addedCount} module(s).");
                EditorUtility.DisplayDialog(
                    "Modules Added",
                    $"Successfully added {addedCount} module(s) to PlayerStateMachine.\n\n" +
                    "Check the Player Modules list in the inspector.",
                    "OK"
                );
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "No New Modules",
                    $"Found {foundModules.Count} module(s) in project, but all are already assigned.",
                    "OK"
                );
            }
        }
    }
}
