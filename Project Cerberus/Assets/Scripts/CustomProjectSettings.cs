/*
 * Custom settings for our project. The settings are stored in an asset saved at
 * CustomProjectSettingsProvider.settingPath. The settings can be loaded via
 * Resources.Load<CustomProjectSettings>(CustomProjectSettingsProvider.resourcePath).
 */
using UnityEditor;
using UnityEngine;

public class CustomProjectSettings : ScriptableObject
{
    public LevelSequence mainLevelSequence;

    internal static CustomProjectSettings GetOrCreateSettings()
    {
        var settings = AssetDatabase.LoadAssetAtPath<CustomProjectSettings>(CustomProjectSettingsProvider.settingPath);
        if (settings == null)
        {
            // Create settings
            settings = CreateInstance<CustomProjectSettings>();
            // Initialize settings
            settings.mainLevelSequence = null;
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