#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using MistInteractive.ThirdPerson.Player;
using MistInteractive.ThirdPerson.Stats;

namespace MistInteractive.ThirdPersonMovement.Editor
{
    /// <summary>
    /// Editor utility for quick setup of Player Stats modules.
    /// Provides one-click installation of Health, Stamina, Mana, and Progression modules.
    /// </summary>
    public class PlayerStatsInstaller : EditorWindow
    {
        private const string ModulesFolder = "Assets/MistInteractive/ThirdPersonMovement/Modules";

        private PlayerStateMachine selectedPlayer;
        private bool createHealth = true;
        private bool createStamina = true;
        private bool createMana = true;
        private bool createProgression = true;

        [MenuItem("MiST/Quick Setup/Add All Stats Modules")]
        public static void ShowWindow()
        {
            var window = GetWindow<PlayerStatsInstaller>("Stats Module Installer");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        [MenuItem("MiST/Quick Setup/Add All Stats Modules", true)]
        private static bool ValidateShowWindow()
        {
            // Enable menu item if a PlayerStateMachine is selected
            return Selection.activeGameObject != null &&
                   Selection.activeGameObject.GetComponent<PlayerStateMachine>() != null;
        }

        private void OnEnable()
        {
            // Auto-detect PlayerStateMachine from selection
            if (Selection.activeGameObject != null)
            {
                selectedPlayer = Selection.activeGameObject.GetComponent<PlayerStateMachine>();
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Player Stats Module Installer", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Quickly add Health, Stamina, Mana, and Progression modules to your player.", MessageType.Info);

            EditorGUILayout.Space(10);

            // Player selection
            selectedPlayer = (PlayerStateMachine)EditorGUILayout.ObjectField(
                "Player StateMachine",
                selectedPlayer,
                typeof(PlayerStateMachine),
                true);

            if (selectedPlayer == null)
            {
                EditorGUILayout.HelpBox("Select a GameObject with PlayerStateMachine component.", MessageType.Warning);
                return;
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Modules to Create:", EditorStyles.boldLabel);

            createHealth = EditorGUILayout.Toggle("Health Module", createHealth);
            createStamina = EditorGUILayout.Toggle("Stamina Module", createStamina);
            createMana = EditorGUILayout.Toggle("Mana Module", createMana);
            createProgression = EditorGUILayout.Toggle("Progression Module", createProgression);

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Install Selected Modules", GUILayout.Height(40)))
            {
                InstallModules();
            }

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Remove All Stats Modules"))
            {
                if (EditorUtility.DisplayDialog("Remove Stats Modules",
                    "Remove all Health, Stamina, Mana, and Progression modules from this player?",
                    "Remove", "Cancel"))
                {
                    RemoveModules();
                }
            }
        }

        private void InstallModules()
        {
            // Ensure Modules folder exists
            if (!AssetDatabase.IsValidFolder(ModulesFolder))
            {
                Directory.CreateDirectory(ModulesFolder);
                AssetDatabase.Refresh();
            }

            int modulesCreated = 0;
            int modulesAdded = 0;

            if (createHealth)
            {
                var health = FindOrCreateModule<PlayerHealthModule>("PlayerHealthModule");
                if (AddModuleToPlayer(health))
                {
                    modulesCreated++;
                    modulesAdded++;
                }
            }

            if (createStamina)
            {
                var stamina = FindOrCreateModule<PlayerStaminaModule>("PlayerStaminaModule");
                if (AddModuleToPlayer(stamina))
                {
                    modulesCreated++;
                    modulesAdded++;
                }
            }

            if (createMana)
            {
                var mana = FindOrCreateModule<PlayerManaModule>("PlayerManaModule");
                if (AddModuleToPlayer(mana))
                {
                    modulesCreated++;
                    modulesAdded++;
                }
            }

            if (createProgression)
            {
                var progression = FindOrCreateModule<PlayerProgressionModule>("PlayerProgressionModule");
                if (AddModuleToPlayer(progression))
                {
                    modulesCreated++;
                    modulesAdded++;
                }
            }

            if (modulesAdded > 0)
            {
                EditorUtility.SetDirty(selectedPlayer);
                EditorUtility.DisplayDialog("Success",
                    $"Successfully added {modulesAdded} module(s) to {selectedPlayer.name}!",
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Info",
                    "All selected modules are already added to this player.",
                    "OK");
            }
        }

        private T FindOrCreateModule<T>(string fileName) where T : PlayerModule
        {
            // Try to find existing asset
            string assetPath = $"{ModulesFolder}/{fileName}.asset";
            T module = AssetDatabase.LoadAssetAtPath<T>(assetPath);

            if (module == null)
            {
                // Create new asset
                module = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(module, assetPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"[PlayerStatsInstaller] Created new module: {assetPath}");
            }

            return module;
        }

        private bool AddModuleToPlayer(PlayerModule module)
        {
            // Use SerializedObject to check if module already exists and add it
            SerializedObject so = new SerializedObject(selectedPlayer);
            SerializedProperty modulesProp = so.FindProperty("modules");

            // Check if module already exists
            for (int i = 0; i < modulesProp.arraySize; i++)
            {
                var existingModule = modulesProp.GetArrayElementAtIndex(i).objectReferenceValue;
                if (existingModule == module)
                {
                    Debug.Log($"[PlayerStatsInstaller] Module {module.name} already exists in player");
                    return false;
                }
            }

            // Add module
            modulesProp.InsertArrayElementAtIndex(modulesProp.arraySize);
            modulesProp.GetArrayElementAtIndex(modulesProp.arraySize - 1).objectReferenceValue = module;

            so.ApplyModifiedProperties();

            Debug.Log($"[PlayerStatsInstaller] Added {module.name} to {selectedPlayer.name}");
            return true;
        }

        private void RemoveModules()
        {
            SerializedObject so = new SerializedObject(selectedPlayer);
            SerializedProperty modulesProp = so.FindProperty("modules");

            // Remove stats modules (reverse iteration for safe removal)
            for (int i = modulesProp.arraySize - 1; i >= 0; i--)
            {
                var element = modulesProp.GetArrayElementAtIndex(i);
                var module = element.objectReferenceValue as PlayerModule;

                if (module is PlayerHealthModule ||
                    module is PlayerStaminaModule ||
                    module is PlayerManaModule ||
                    module is PlayerProgressionModule)
                {
                    modulesProp.DeleteArrayElementAtIndex(i);
                }
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(selectedPlayer);

            EditorUtility.DisplayDialog("Success",
                "Removed all Stats modules from player.",
                "OK");
        }
    }
}
#endif
