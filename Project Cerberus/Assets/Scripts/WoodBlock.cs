﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoodBlock : BasicBlock
{
    public class WoodBlockUndoData : UndoData
    {
        public WoodBlock wood;
        public Vector2Int position;
        public bool burned;

        public WoodBlockUndoData(WoodBlock wood, Vector2Int position, bool burned)
        {
            this.wood = wood;
            this.position = position;
            this.burned = burned;
        }

        public override void Load()
        {
            wood.SetFieldsToShotPreset(burned);
            wood.MoveForUndo(position);
        }
    }

    [SerializeField] private Sprite wholeSprite;
    [SerializeField] private Sprite destroyedSprite;
    private SpriteRenderer _spriteRenderer;

    protected WoodBlock()
    {
        entityRules = "Wood blocks burn up when they are shot by Kahuna.";
        pushableByFireball = false;
        interactsWithFireball = true;
    }

    private void Awake()
    {
        base.Awake();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void OnShotByKahuna()
    {
        SetFieldsToShotPreset(true);
    }

    public void SetFieldsToShotPreset(bool shot)
    {
        if (shot)
        {
            _spriteRenderer.sprite = destroyedSprite;
            SetCollisionsEnabled(false);
            landable = true;
            jumpable = false;
            interactsWithFireball = false;
            pushableByStandardMove = false;
            pushableByJacksMultiPush = false;
            pushableByJacksSuperPush = false;
        }
        else
        {
            _spriteRenderer.sprite = wholeSprite;
            SetCollisionsEnabled(true);
            landable = false;
            jumpable = true;
            interactsWithFireball = true;
            pushableByStandardMove = true;
            pushableByJacksMultiPush = true;
            pushableByJacksSuperPush = true;
        }
    }

    public override UndoData GetUndoData()
    {
        var undoData = new WoodBlockUndoData(this, position, burned: !collisionsEnabled);
        return undoData;
    }
}