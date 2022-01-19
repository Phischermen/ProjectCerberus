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
        public bool wantsToClose;

        public GateUndoData(Gate gate, bool open, bool wantsToClose)
        {
            this.gate = gate;
            this.open = open;
            this.wantsToClose = wantsToClose;
        }

        public override void Load()
        {
            gate._wantsToClose = wantsToClose;
            if (open)
            {
                gate.OpenGate();
            }
            else
            {
                gate.CloseGate();
            }
        }

    }
    
    private bool _wantsToClose;
    [ShowInTileInspector] public bool open;
    private bool _lastOpen;
    [SerializeField] private Sprite openSprite;
    [SerializeField] private Sprite closeSprite;
    private SpriteRenderer _spriteRenderer;

    public AudioSource openAudioSource;
    public AudioSource closeAudioSource;
    public Gate()
    {
        entityRules = "Can be opened and closed via switches and levers. Jumpable when closed and landable when open.";
        isStatic = true;
    }

    public override UndoData GetUndoData()
    {
        var undoData = new GateUndoData(this, open, _wantsToClose);
        return undoData;
    }

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
        if (_wantsToClose)
        {
            // Set collision parameters in preparation for collision check.
            stopsPlayer = true;
            stopsBlock = true;
            if (!CollidesWithAny(currentCell.puzzleEntities))
            {
                CloseGate();
            }
            else
            {
                // Reset collision parameters.
                stopsBlock = false;
                stopsPlayer = false;
            }
        }
    }

    private void LateUpdate()
    {
        if (_lastOpen != open)
        {
            if (open)
            {
                PlaySfx(openAudioSource);
            }
            else
            {
                PlaySfx(closeAudioSource);
            }
        }

        _lastOpen = open;
    }

    public void OpenGate()
    {
        _wantsToClose = false;
        _spriteRenderer.sprite = openSprite;
        SetFieldsToOpenPreset();
    }

    public void RequestCloseGate()
    {
        _wantsToClose = true;
    }

    private void CloseGate()
    {
        _wantsToClose = false;
        _spriteRenderer.sprite = closeSprite;
        SetFieldsToClosedPreset();
    }

    protected virtual void SetFieldsToOpenPreset()
    {
        open = true;
        stopsBlock = false;
        stopsPlayer = false;
        landableScore = 0;
        jumpable = false;
    }

    protected virtual void SetFieldsToClosedPreset()
    {
        open = false;
        stopsBlock = true;
        stopsPlayer = true;
        landableScore = -1;
        jumpable = true;
    }
}