using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : PuzzleEntity
{

    public class GateUndoData : UndoData
    {
        public Gate gate;
        public bool open;

        public GateUndoData(Gate gate, bool open)
        {
            this.gate = gate;
            this.open = open;
        }

        public override void Load()
        {
            open = !open;
        }


    }
    private bool wantsToClose;
    [ShowInTileInspector] public bool open;
    [SerializeField] private Sprite openSprite;
    [SerializeField] private Sprite closeSprite;
    private SpriteRenderer _spriteRenderer;

    protected override void Awake()
    {
        base.Awake();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (open)
        {
            OpenGate();
        }
        else
        {
            RequestCloseGate();
        }
    }

    private void Update()
    {
        if (wantsToClose)
        {
            var myCell = puzzle.GetCell(position);
            if (!CollidesWithAny(myCell.puzzleEntities))
            {
                CloseGate();
            }
        }
    }

    public void OpenGate()
    {
        wantsToClose = false;
        _spriteRenderer.sprite = openSprite;
        SetFieldsToOpenPreset();
    }

    public void RequestCloseGate()
    {
        wantsToClose = true;
    }

    private void CloseGate()
    {
        wantsToClose = false;
        _spriteRenderer.sprite = closeSprite;
        SetFieldsToClosedPreset();
    }

    private void SetFieldsToOpenPreset()
    {
        open = true;
        stopsBlock = false;
        stopsPlayer = false;
        landable = true;
    }

    private void SetFieldsToClosedPreset()
    {
        open = false;
        stopsBlock = true;
        stopsPlayer = true;
        landable = false;
    }
}