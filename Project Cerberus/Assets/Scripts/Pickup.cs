using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : PuzzleEntity
{

    public class PickupUndoData : UndoData
    {
        Pickup pickup;
        bool pickedUp;

        public  PickupUndoData(Pickup pickup, bool pickedUp)
        {
            this.pickup = pickup;
            this.pickedUp = pickedUp;
        }

        public override void Load()
        {
            pickup.SetFieldsToCollectedPreset(pickedUp);
        }
    }
    public Color collectedColor;
    [HideInInspector,ShowInTileInspector] public bool collected;
    public Pickup()
    {
        entityRules = "A bonus pickup. Collecting this for a surprise reward.";
        landable = true;
    }

    public override UndoData GetUndoData()
    {
        var undoData = new PickupUndoData(this, collected);
        return undoData;
    }

    public override void OnEnterCollisionWithEntity(PuzzleEntity other)
    {
        if (collected) return;
        if (other is Cerberus cerberus)
        {
            manager.collectedStar = true;
            SetFieldsToCollectedPreset(true);
        }
    }

    public void SetFieldsToCollectedPreset(bool pickedUp)
    {
        GetComponent<SpriteRenderer>().color = pickedUp ? collectedColor : Color.white;

    }
}
