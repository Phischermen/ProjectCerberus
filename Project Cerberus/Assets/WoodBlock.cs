using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoodBlock : BasicBlock
{

    public class WoodBlockUndoData : UndoData
    {

        public WoodBlock wood;
        public bool burned;

        public WoodBlockUndoData(WoodBlock wood, bool burned)
        {
            this.wood = wood;
            this.burned = burned;
        }

        public override void Load()
        {
            
        }

    }

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

    public void BurntBlock(bool burn)
    {
        if (burn == true)
        {
            GetComponent<SpriteRenderer>().sprite = destroyedSprite;
            SetCollisionsEnabled(false);
            landable = true;
            pushable = false;
        }

        if (burn == false)
        {
            SetCollisionsEnabled(true);
            landable = false;
            pushable = true;
        }
    }

}
