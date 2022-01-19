/*
 * GameAssets is a collection of assets bundled as a ScriptableObject. These assets can be accessed at runtime, and set
 * at editor-time.
 */
using UnityEngine;

[CreateAssetMenu(menuName = "GameAssets")]
public class GameAssets : ScriptableObject
{
    public GameObject textPopupPrefab;
    public DialogueDatabaseAsset dialogueDatabaseAsset;

    private static GameAssets _i;

    public static GameAssets i
    {
        get
        {
            if (_i == null)
            {
                _i = Instantiate(Resources.Load<GameAssets>("GameAssets"));
            }

            return _i;
        }
    }
}