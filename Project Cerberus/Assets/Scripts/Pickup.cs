using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : PuzzleEntity
{
    public Color collectedColor;
    [HideInInspector,ShowInTileInspector] public bool collected;
    public Pickup()
    {
        entityRules = "A bonus pickup. Collecting this for a surprise reward.";
        landable = true;
    }

    public override void OnEnterCollisionWithEntity(PuzzleEntity other)
    {
        if (collected) return;
        if (other is Cerberus cerberus)
        {
            manager.collectedStar = true;
            GetComponent<SpriteRenderer>().color = collectedColor;
        }
    }
}
