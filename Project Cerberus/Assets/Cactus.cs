using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cactus : BasicBlock
{
    [SerializeField] private Sprite wholeSprite;
    [SerializeField] private Sprite destroyedSprite;
    protected Cactus()
    {
        entityRules = "A thorny but fragile obstacle. Cannot be pushed directly";
        pushableByStandardMove = false;
        pushableByJacksSuperPush = false;
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
        pushableByJacksMultiPush = false;
    }
}
