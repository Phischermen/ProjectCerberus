/*
 * TrophyData is used mainly to get the appropriate sprite to render a trophy for the UI, but it also has a helper
 * method to determine what kind of trophy a certain 'score' earns.
 */

using UnityEngine;

[CreateAssetMenu]
public class TrophyData : ScriptableObject
{
    public static char goldCode = '4';
    public static char silverCode = '3';
    public static char bronzeCode = '2';
    public static char nopeCode = '1';
    public static string initialTrophyCode = "111";

    public static char GetTropheyCode(float score, float parScore, float silverBracket, float bronzeBracket)
    {
        var scoreDifference = score - parScore;
        if (scoreDifference < 0)
        {
            return goldCode;
        }

        if (scoreDifference < silverBracket)
        {
            return silverCode;
        }

        if (scoreDifference < bronzeBracket)
        {
            return bronzeCode;
        }

        return nopeCode;
    }

    public Sprite GetSpriteToDisplay(char code)
    {
        switch (code)
        {
            case '4': return gold;
            case '3': return silver;
            case '2': return bronze;
            case '1': return nope;
            default: return nope;
        }
    }

    public Sprite gold;
    public Sprite silver;
    public Sprite bronze;
    public Sprite nope;
}