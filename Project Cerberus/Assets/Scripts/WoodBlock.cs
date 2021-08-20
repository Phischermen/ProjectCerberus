using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoodBlock : BasicBlock
{
    [SerializeField] private Sprite wholeSprite;
    [SerializeField] private Sprite destroyedSprite;

    protected WoodBlock()
    {
        interactsWithFireball = true;
    }

    public override void OnShotByKahuna()
    {
        GetComponent<SpriteRenderer>().sprite = destroyedSprite;
        SetCollisionsEnabled(false);
        landable = true;
        pushable = false;
    }
}
