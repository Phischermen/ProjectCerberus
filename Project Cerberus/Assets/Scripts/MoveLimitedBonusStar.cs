using UnityEditor;
using UnityEngine;

public class MoveLimitedBonusStar : BonusStar
{
    public int movesUntilUnavailable;

    public MoveLimitedBonusStar()
    {
        entityRules = "A bonus bonusStar. Disappears after a certain number of moves.";
    }

#if UNITY_EDITOR
    public override void DrawControlsForInspectorGUI()
    {
        movesUntilUnavailable =
            EditorGUILayout.IntSlider("Moves until bonus star unavailable.", movesUntilUnavailable, 0, 100);
    }
#endif

    public override string GetStatusMessageForUI()
    {
        if (collected)
        {
            return "Bonus star collected!";
        }

        if (unavailable)
        {
            return "Bonus star can no longer be obtained.";
        }

        if (customBonusStarMessageAvailable == "")
        {
            return $"Bonus star disappears in {movesUntilUnavailable - manager.move} move(s).";
        }

        return string.Format(customBonusStarMessageAvailable, movesUntilUnavailable - manager.move);
    }

    public override void OnPlayerMadeMove()
    {
        if (collected)
        {
            SetFieldsToCollectedPreset();
        }
        else if (manager.move >= movesUntilUnavailable || unavailable)
        {
            SetFieldsToUnavailablePreset();
        }
        else
        {
            SetFieldsToUncollectedPreset();
            var popup = TextPopup.Create((movesUntilUnavailable - manager.move).ToString(), Color.yellow);
            popup.transform.position = transform.position;
            popup.PlayRiseAndFadeAnimation();
        }
    }
}