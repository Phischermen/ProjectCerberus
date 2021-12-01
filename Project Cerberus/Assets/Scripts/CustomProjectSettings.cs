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
    public static string resourcePath = "CustomProjectSettings";
}