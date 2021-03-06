/*
 * Custom settings for our project. The settings are stored in an asset saved at
 * CustomProjectSettingsProvider.settingPath. The settings can be loaded via
 * CustomProjectSetting's singleton: i.
 */

using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.U2D.Animation;

public class CustomProjectSettings : ScriptableObject
{
    public LevelSequence mainLevelSequence;
    public LevelSequence multiplayerLevelSequence;
    public GameObject puzzleContainerPrefab;

    public GameObject[] puzzleLevelIncludes;

    public GameObject textPopupPrefab;
    public DialogueDatabaseAsset dialogueDatabaseAsset;

    public AnimationCurve defaultTalkAnimationCurve;
    
    public static string resourcePath = "CustomProjectSettings";
    
    private static CustomProjectSettings _i;
    public SpriteLibraryAsset normalSpriteLibrary;
    public SpriteLibraryAsset alternateSpriteLibrary;

    public static CustomProjectSettings i
    {
        get
        {
            if (_i == null)
            {
                _i = Instantiate(Resources.Load<CustomProjectSettings>(resourcePath));
            }

            return _i;
        }
    }
}