using UnityEngine;
using UnityEditor;
using MistInteractive.ThirdPerson.Player;
using System.IO;

namespace MistInteractive.ThirdPerson.Editor
{
    /// <summary>
    /// Editor tool for quickly setting up the Interaction module.
    /// Accessible via menu: MiST/Quick Setup/Add Interaction Module
    /// </summary>
    public class InteractionModuleInstaller : EditorWindow
    {
        private PlayerStateMachine selectedStateMachine;
        private PlayerInteractionModule interactionModule;

        private const string MODULE_FOLDER = "Assets/MistInteractive/ThirdPersonMovement/Modules";
        private const string MODULE_FILE = "InteractionModule.asset";

        [MenuItem("MiST/Quick Setup/Add Interaction Module")]
        public static void ShowWindow()
        {
            var window = GetWindow<InteractionModuleInstaller>("Interaction Module Setup");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnEnable()
        {
            // Try to find PlayerStateMachine in scene
            selectedStateMachine = FindFirstObjectByType<PlayerStateMachine>();

            // Try to find existing InteractionModule
            interactionModule = FindInteractionModule();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Interaction Module Quick Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This tool will help you quickly set up the Interaction module for your player.\n\n" +
                "It will:\n" +
                "1. Create an InteractionModule asset (if it doesn't exist)\n" +
                "2. Add it to your PlayerStateMachine\n" +
                "3. Provide instructions for setting up UI",
                MessageType.Info
            );

            EditorGUILayout.Space(10);

            // PlayerStateMachine selection
            EditorGUILayout.LabelField("Step 1: Select PlayerStateMachine", EditorStyles.boldLabel);
            selectedStateMachine = (PlayerStateMachine)EditorGUILayout.ObjectField(
                "Player State Machine",
                selectedStateMachine,
                typeof(PlayerStateMachine),
                true
            );

            if (selectedStateMachine == null)
            {
                EditorGUILayout.HelpBox("Please select a PlayerStateMachine from your scene.", MessageType.Warning);
                return;
            }

            EditorGUILayout.Space(10);

            // Module status
            EditorGUILayout.LabelField("Step 2: Install Module", EditorStyles.boldLabel);

            if (interactionModule == null)
            {
                EditorGUILayout.HelpBox("InteractionModule asset not found in project.", MessageType.Warning);

                if (GUILayout.Button("Create InteractionModule Asset", GUILayout.Height(30)))
                {
                    interactionModule = CreateInteractionModule();
                    if (interactionModule != null)
                    {
                        EditorUtility.DisplayDialog(
                            "Module Created",
                            $"InteractionModule asset created at:\n{AssetDatabase.GetAssetPath(interactionModule)}",
                            "OK"
                        );
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox($"InteractionModule found at:\n{AssetDatabase.GetAssetPath(interactionModule)}", MessageType.Info);

                // Check if already installed
                bool alreadyInstalled = IsModuleInstalled();

                if (alreadyInstalled)
                {
                    EditorGUILayout.HelpBox("InteractionModule is already installed on this PlayerStateMachine.", MessageType.Info);

                    if (GUILayout.Button("Remove Interaction Module", GUILayout.Height(25)))
                    {
                        if (EditorUtility.DisplayDialog(
                            "Remove Module",
                            "Are you sure you want to remove the InteractionModule from this PlayerStateMachine?",
                            "Yes",
                            "No"))
                        {
                            RemoveModule();
                        }
                    }
                }
                else
                {
                    if (GUILayout.Button("Add Interaction Module to PlayerStateMachine", GUILayout.Height(30)))
                    {
                        AddModuleToStateMachine();
                    }
                }
            }

            EditorGUILayout.Space(10);

            // UI setup instructions
            EditorGUILayout.LabelField("Step 3: Setup UI (Optional)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "To display interaction prompts:\n\n" +
                "1. Create a Canvas in your scene\n" +
                "2. Add InteractionUIController component to a GameObject\n" +
                "3. Assign UI elements (TextMeshPro for prompts, etc.)\n" +
                "4. The UI will automatically connect to the module",
                MessageType.Info
            );

            EditorGUILayout.Space(10);

            // Settings
            if (interactionModule != null)
            {
                EditorGUILayout.LabelField("Module Settings", EditorStyles.boldLabel);
                UnityEditor.Editor moduleEditor = UnityEditor.Editor.CreateEditor(interactionModule);
                moduleEditor.OnInspectorGUI();
            }
        }

        private PlayerInteractionModule FindInteractionModule()
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(PlayerInteractionModule)}");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<PlayerInteractionModule>(path);
            }
            return null;
        }

        private PlayerInteractionModule CreateInteractionModule()
        {
            // Ensure folder exists
            if (!AssetDatabase.IsValidFolder(MODULE_FOLDER))
            {
                Directory.CreateDirectory(MODULE_FOLDER);
                AssetDatabase.Refresh();
            }

            // Create asset
            var module = ScriptableObject.CreateInstance<PlayerInteractionModule>();
            var fullPath = Path.Combine(MODULE_FOLDER, MODULE_FILE);

            AssetDatabase.CreateAsset(module, fullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[InteractionModuleInstaller] Created InteractionModule at {fullPath}");
            return module;
        }

        private bool IsModuleInstalled()
        {
            var serializedObj = new SerializedObject(selectedStateMachine);
            var modulesProp = serializedObj.FindProperty("modules");

            for (int i = 0; i < modulesProp.arraySize; i++)
            {
                var module = modulesProp.GetArrayElementAtIndex(i).objectReferenceValue;
                if (module == interactionModule)
                    return true;
            }

            return false;
        }

        private void AddModuleToStateMachine()
        {
            var serializedObj = new SerializedObject(selectedStateMachine);
            var modulesProp = serializedObj.FindProperty("modules");

            // Check for duplicates
            if (IsModuleInstalled())
            {
                EditorUtility.DisplayDialog(
                    "Already Installed",
                    "InteractionModule is already installed on this PlayerStateMachine.",
                    "OK"
                );
                return;
            }

            // Add module
            modulesProp.arraySize++;
            var newElement = modulesProp.GetArrayElementAtIndex(modulesProp.arraySize - 1);
            newElement.objectReferenceValue = interactionModule;

            serializedObj.ApplyModifiedProperties();

            Debug.Log("[InteractionModuleInstaller] Added InteractionModule to PlayerStateMachine.");
            EditorUtility.DisplayDialog(
                "Module Added",
                "InteractionModule has been successfully added to the PlayerStateMachine!\n\n" +
                "Don't forget to set up the UI if you want visual prompts.",
                "OK"
            );
        }

        private void RemoveModule()
        {
            var serializedObj = new SerializedObject(selectedStateMachine);
            var modulesProp = serializedObj.FindProperty("modules");

            for (int i = modulesProp.arraySize - 1; i >= 0; i--)
            {
                var module = modulesProp.GetArrayElementAtIndex(i).objectReferenceValue;
                if (module == interactionModule)
                {
                    modulesProp.DeleteArrayElementAtIndex(i);
                }
            }

            serializedObj.ApplyModifiedProperties();

            Debug.Log("[InteractionModuleInstaller] Removed InteractionModule from PlayerStateMachine.");
            EditorUtility.DisplayDialog(
                "Module Removed",
                "InteractionModule has been removed from the PlayerStateMachine.",
                "OK"
            );
        }
    }
}
