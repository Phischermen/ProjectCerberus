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
            else if(pickup.manager.move > pickup.manager.maxMovesUntilStarLoss)
            {
                pickup.SetFieldsToUnavailablePreset();
            }
            else
            {
                pickup.SetFieldsToUncollectedPreset();
            }
        }
    }

    public Color collectedColor;
    public Color unavailableColor;
    [HideInInspector, ShowInTileInspector] public bool collected;
    private SpriteRenderer _spriteRenderer;

    public Pickup()
    {
        entityRules = "A bonus pickup. Collecting this for a surprise reward.";
        landableScore = 0;
    }

    private void Awake()
    {
        base.Awake();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override UndoData GetUndoData()
    {
        var undoData = new PickupUndoData(this, collected);
        return undoData;
    }

    public override void OnEnterCollisionWithEntity(PuzzleEntity other)
    {
        if (collected || manager.move > manager.maxMovesUntilStarLoss) return;
        if (other is Cerberus)
        {
            manager.collectedStar = true;
            SetFieldsToCollectedPreset();
        }
    }

    public void SetFieldsToUncollectedPreset()
    {
        _spriteRenderer.color = Color.white;
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