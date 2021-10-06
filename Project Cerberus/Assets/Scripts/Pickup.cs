using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : PuzzleEntity
{
    public class PickupUndoData : UndoData
    {
        Pickup pickup;
        bool collected;

        public PickupUndoData(Pickup pickup, bool collected)
        {
            this.pickup = pickup;
            this.collected = collected;
        }

        public override void Load()
        {
            if (collected)
            {
                pickup.SetFieldsToCollectedPreset();
            }
            else if (!pickup.manager.infinteMovesTilStarLoss && pickup.manager.move > pickup.manager.maxMovesUntilStarLoss)
            {
                pickup.SetFieldsToUnavailablePreset();
            }
            else
            {
                pickup.SetFieldsToUncollectedPreset();
            }
        }
    }

    public Color initialColor;
    public Color collectedColor;
    public Color unavailableColor;
    [HideInInspector, ShowInTileInspector] public bool collected;
    private SpriteRenderer _spriteRenderer;

    public Pickup()
    {
        entityRules = "A bonus pickup. Collect this for a surprise reward.";
        landableScore = 0;
    }

    private void Awake()
    {
        base.Awake();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        initialColor = _spriteRenderer.color;
    }

    public override UndoData GetUndoData()
    {
        var undoData = new PickupUndoData(this, collected);
        return undoData;
    }

    public override void OnPlayerMadeMove()
    {
        if (collected)
        {
            SetFieldsToCollectedPreset();
        }
        else if (!manager.infinteMovesTilStarLoss && manager.move > manager.maxMovesUntilStarLoss)
        {
            SetFieldsToUnavailablePreset();
        }
        else
        {
            SetFieldsToUncollectedPreset();
        }
    }

    public override void OnEnterCollisionWithEntity(PuzzleEntity other)
    {
        if (collected || !manager.infinteMovesTilStarLoss && manager.move > manager.maxMovesUntilStarLoss) return;
        if (other is Cerberus)
        {
            collected = true;
            manager.collectedStar = true;
        }
    }

    public void SetFieldsToUncollectedPreset()
    {
        _spriteRenderer.color = initialColor;
        collected = false;
    }

    public void SetFieldsToCollectedPreset()
    {
        _spriteRenderer.color = collectedColor;
        collected = true;
    }

    public void SetFieldsToUnavailablePreset()
    {
        _spriteRenderer.color = unavailableColor;
        collected = false;
    }
}