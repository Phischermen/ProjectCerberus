using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoodBlock : BasicBlock
{
    [SerializeField] private Sprite wholeSprite;
    [SerializeField] private Sprite destroyedSprite;

    protected WoodBlock()
    {
        entityRules = "Wood blocks burn up when they are shot by Kahuna.";
        pushableByFireball = false;
        interactsWithFireball = true;
    }

    public override void OnShotByKahuna()
    {
        GetComponent<SpriteRenderer>().sprite = destroyedSprite;
        SetCollisionsEnabled(false);
        landable = true;
        jumpable = false;
        interactsWithFireball = false;
        pushableByFireball = false;
        pushableByStandardMove = false;
        pushableByJacksMultiPush = false;
        pushableByJacksSuperPush = false;
    }
}
