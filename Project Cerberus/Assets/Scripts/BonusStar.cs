/*
 * The BonusStar is a bonusStar that rewards the player with a +1 star rating upon finishing the level. Every level should
 * have one. BonusStars can be made unavailable, meaning the player can no longer pick them up and reap their reward.
 * By default, bonus stars will never be made unavailable. Their are derived classes however such as
 * MoveLimitedBonusStar, which becomes unavailable after a set number of moves.
 */

using UnityEditor;
using UnityEngine;

public class BonusStar : PuzzleEntity
{
    public class BonusStarUndoData : UndoData
    {
        BonusStar _bonusStar;
        bool collected;
        bool unavailable;

        public BonusStarUndoData(BonusStar bonusStar, bool collected, bool unavailable)
        {
            this._bonusStar = bonusStar;
            this.collected = collected;
            this.unavailable = unavailable;
        }

        public override void Load()
        {
            if (collected)
            {
                _bonusStar.SetFieldsToCollectedPreset();
            }
            else if (unavailable)
            {
                _bonusStar.SetFieldsToUnavailablePreset();
            }
            else
            {
                _bonusStar.SetFieldsToUncollectedPreset();
            }
        }
    }

    public Color initialColor;
    public Color collectedColor;
    public Color unavailableColor;
    [HideInInspector, ShowInTileInspector] public bool collected;
    [HideInInspector, ShowInTileInspector] public bool unavailable;

    public BonusStar()
    {
        entityRules = "A bonus bonusStar. Collect this for a surprise reward.";
        landableScore = 0;
    }

    private new void Awake()
    {
        base.Awake();
        initialColor = spriteRenderer.color;
    }

    public override UndoData GetUndoData()
    {
        var undoData = new BonusStarUndoData(this, collected, unavailable);
        return undoData;
    }

    // This virtual method is available to conveniently edit the bonus star with GameManagerEditor.
    // NOTE: Game will not build if this method (and overrides) are not wrapped with UNITY_EDITOR directive.
#if UNITY_EDITOR
    public virtual void DrawControlsForInspectorGUI()
    {
        EditorGUILayout.LabelField("No options for bonus star.");
    }
#endif

    // PuzzleUI will display the returned string for the player. 
    public virtual string GetStatusMessageForUI()
    {
        return "";
    }

    public override void OnEnterCollisionWithEntity(PuzzleEntity other)
    {
        if (collected || unavailable) return;
        if (other is Cerberus)
        {
            collected = true;
            manager.collectedStar = true;
            SetFieldsToCollectedPreset();
        }
    }

    public void SetFieldsToUncollectedPreset()
    {
        spriteRenderer.color = initialColor;
        collected = false;
        unavailable = false;
    }

    public void SetFieldsToCollectedPreset()
    {
        spriteRenderer.color = collectedColor;
        collected = true;
        unavailable = true;
    }

    public void SetFieldsToUnavailablePreset()
    {
        spriteRenderer.color = unavailableColor;
        collected = false;
        unavailable = true;
    }
}