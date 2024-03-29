﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoodBlock : BasicBlock
{
    public class WoodBlockStateData : BasicBlockStateData
    {
        public WoodBlock wood;
        public bool burned => booleans[0];

        public WoodBlockStateData(WoodBlock wood, Vector2Int position, bool burned, bool inHole) : base(wood, position, inHole)
        {
            this.wood = wood;
            booleans[0] = burned;
        }

        public override void Load()
        {
            base.Load();
            wood.SetFieldsToShotPreset(burned);
            wood._spriteRenderer.sprite = burned ? wood.destroyedSprite : wood.wholeSprite;
        }
    }

    [SerializeField] private Sprite wholeSprite;
    [SerializeField] private Sprite destroyedSprite;
    private SpriteRenderer _spriteRenderer;
    public bool isDestroyed;
    public AudioSource woodHitSfx;

    protected WoodBlock()
    {
        entityRules = "Wood blocks burn up when they are shot by Kahuna. The ashes make a nice cushion for Cerberus.";
        pushableByFireball = false;
        interactsWithFireball = true;
    }

    private new void Awake()
    {
        base.Awake();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void OnShotByKahuna()
    {
        SetFieldsToShotPreset(true);
    }

    public override void OnShotByKahunaVisually()
    {
        spriteRenderer.sprite = destroyedSprite;
        woodHitSfx.Play();
    }

    public virtual void SetFieldsToShotPreset(bool shot)
    {
        if (shot)
        {
            isDestroyed = true;
            SetCollisionsEnabled(false);
            stopsPlayer = false;
            stopsBlock = false;
            isBlock = false;
            landableScore = 1;
            jumpable = false;
            interactsWithFireball = false;
            pullable = false;
            pushableByStandardMove = false;
            pushableByJacksMultiPush = false;
            pushableByJacksSuperPush = false;
        }
        else
        {
            isDestroyed = false;
            SetCollisionsEnabled(true);
            stopsPlayer = true;
            stopsBlock = true;
            isBlock = true;
            landableScore = -1;
            jumpable = true;
            interactsWithFireball = true;
            pullable = true;
            pushableByStandardMove = true;
            pushableByJacksMultiPush = true;
            pushableByJacksSuperPush = true;
        }
    }

    public override StateData GetUndoData()
    {
        var undoData = new WoodBlockStateData(this, position, burned: !stopsPlayer, inHole);
        return undoData;
    }
}