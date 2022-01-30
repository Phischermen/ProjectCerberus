/*
 * Settings provider for CustomProjectSettings. The settings are stored in an asset saved at settingPath. The settings
 * can be loaded via Resources.Load<CustomProjectSettings>(CustomProjectSettingsProvider.resourcePath).
 */

using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    class CustomProjectSettingsProvider : SettingsProvider
    {
        public CustomProjectSettingsProvider(string path, SettingsScope scope = SettingsScope.Project) : base(path, scope)
        {
        }

        private SerializedObject _settings;
        public static string settingPath = "Assets/Resources/CustomProjectSettings.asset";

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            _settings = GetSerializedSettings();
        }

        public override void OnGUI(string searchContext)
        {
            // Add property field to set mainLevelSequence
            EditorGUILayout.PropertyField(_settings.FindProperty(nameof(CustomProjectSettings.mainLevelSequence)));
            EditorGUILayout.PropertyField(_settings.FindProperty(nameof(CustomProjectSettings.dialogueDatabaseAsset)));
            EditorGUILayout.LabelField("Prefabs");
            EditorGUILayout.PropertyField(_settings.FindProperty(nameof(CustomProjectSettings.puzzleContainerPrefab)));
            EditorGUILayout.PropertyField(_settings.FindProperty(nameof(CustomProjectSettings.puzzleLevelIncludes)));
            EditorGUILayout.PropertyField(_settings.FindProperty(nameof(CustomProjectSettings.textPopupPrefab)));
            // Apply changes
            _settings.ApplyModifiedProperties();
        }

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            if (SettingsAvailable())
            {
                var provider = new CustomProjectSettingsProvider("Project/Custom Project Settings");
                return provider;
            }

            GetOrCreateSettings();
            return null;
        }

        private static bool SettingsAvailable()
        {
            return File.Exists(settingPath);
        }
        
        internal static CustomProjectSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<CustomProjectSettings>(CustomProjectSettingsProvider.settingPath);
            if (settings == null)
            {
                // Create settings
                settings = ScriptableObject.CreateInstance<CustomProjectSettings>();
                // Initialize settings
                settings.mainLevelSequence = null;
                settings.puzzleContainerPrefab = null;
                // Create the settings asset
                AssetDatabase.CreateAsset(settings, CustomProjectSettingsProvider.settingPath);
                // Save the asset
                AssetDatabase.SaveAssets();
            }

            return settings;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
    }
}