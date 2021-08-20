using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : PuzzleEntity
{
    public Color collectedColor;
    [HideInInspector,ShowInTileInspector] public bool collected;
    public Pickup()
    {
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
