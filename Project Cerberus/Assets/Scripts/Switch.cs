﻿using System;
using UnityEngine;
using UnityEngine.Events;

public class Switch : PuzzleEntity
{
    public class SwitchStateData : StateData
    {
        public Switch lever;
        public bool flipped => booleans[0];

        public SwitchStateData(Switch lever, bool flipped)
        {
            this.lever = lever;
            booleans[0] = flipped;
        }

        public override void Load()
        {
            lever.isPressed = flipped;
            lever._lastIsPressed = flipped; // Prevents unintended callback invocation.
            lever.SwitchOnVisually(flipped);
        }
    }
    
    public UnityEvent onPressed;
    public UnityEvent onReleased;

    [HideInInspector, ShowInTileInspector] public bool isPressed;
    private bool _lastIsPressed;
    [SerializeField] private Sprite depressedSprite;
    [SerializeField] private Sprite raisedSprite;
    private SpriteRenderer _spriteRenderer;
    private LineRenderer _lineRenderer;
    
    public AudioSource pressedAudioSource;
    public AudioSource releasedAudioSource;
    protected Switch()
    {
        entityRules = "Switches control other objects. A switch must be held down with an object.";
        isStatic = true;
        landableScore = 0;
    }

    public override StateData GetUndoData()
    {
        var undoData = new SwitchStateData(this, isPressed);
        return undoData;
    }

    protected override void Awake()
    {
        base.Awake();
        _lineRenderer = GetComponent<LineRenderer>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _lineRenderer.startColor = Color.red;
        _lineRenderer.endColor = Color.red;
    }

    private void LateUpdate()
    {
        if (_lastIsPressed != isPressed)
        {
            if (isPressed)
            {
                PlaySfx(pressedAudioSource);
                SwitchOnVisually(true);
                onPressed.Invoke();
            }
            else
            {
                PlaySfx(releasedAudioSource);
                SwitchOnVisually(false);
                onReleased.Invoke();
            }
        }

        _lastIsPressed = isPressed;
    }

    public override void OnEnterCollisionWithEntity(PuzzleEntity other)
    {
        if (isPressed || other.ignoresSwitch) return;
        isPressed = true;
        
    }

    public override void OnExitCollisionWithEntity(PuzzleEntity other)
    {
        if (!isPressed || other.ignoresSwitch) return;
        isPressed = false;
        
    }

    public void SwitchOnVisually(bool on)
    {
        if (on == true)
        {
            _spriteRenderer.sprite = depressedSprite;
            _lineRenderer.startColor = Color.green;
            _lineRenderer.endColor = Color.green;
        }

        if (on == false)
        {
            _spriteRenderer.sprite = raisedSprite;
            _lineRenderer.startColor = Color.red;
            _lineRenderer.endColor = Color.red;
        }
    }
}