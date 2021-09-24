/*
 * Settings provider for CustomProjectSettings. The settings are stored in an asset saved at settingPath. The settings
 * can be loaded via Resources.Load<CustomProjectSettings>(CustomProjectSettingsProvider.resourcePath).
 */
using System.IO;
using UnityEditor;
using UnityEngine.UIElements;

class CustomProjectSettingsProvider : SettingsProvider
{
    public CustomProjectSettingsProvider(string path, SettingsScope scope = SettingsScope.Project) : base(path, scope)
    {
    }

    private SerializedObject _settings;
    public static string settingPath = "Assets/Resources/CustomProjectSettings.asset";
    public static string resourcePath = "CustomProjectSettings";

    public override void OnActivate(string searchContext, VisualElement rootElement)
    {
        _settings = CustomProjectSettings.GetSerializedSettings();
    }

    public override void OnGUI(string searchContext)
    {
        // Add property field to set mainLevelSequence
        EditorGUILayout.PropertyField(_settings.FindProperty(nameof(CustomProjectSettings.mainLevelSequence)));
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

        CustomProjectSettings.GetOrCreateSettings();
        return null;
    }

    private static bool SettingsAvailable()
    {
        return File.Exists(settingPath);
    }
}