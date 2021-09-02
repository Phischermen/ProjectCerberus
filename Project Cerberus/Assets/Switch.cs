﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Switch : PuzzleEntity
{

    public class SwitchUndoData : UndoData
    {

        public Switch lever;
        public bool flipped;

        public SwitchUndoData(Switch lever, bool flipped)
        {
            this.lever = lever;
            this.flipped = flipped;
        }

        public override void Load()
        {
            flipped = !flipped;
        }

    }

    public UnityEvent onPressed;
    public UnityEvent onReleased;

    [HideInInspector, ShowInTileInspector] public bool isPressed;
    [SerializeField] private Sprite depressedSprite;
    [SerializeField] private Sprite raisedSprite;
    private SpriteRenderer _spriteRenderer;
    private LineRenderer _lineRenderer;

    Switch()
    {
        isStatic = true;
        landable = true;
    }

    protected override void Awake()
    {
        base.Awake();
        _lineRenderer = GetComponent<LineRenderer>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _lineRenderer.startColor = Color.red;
        _lineRenderer.endColor = Color.red;
    }

    public override void OnEnterCollisionWithEntity(PuzzleEntity other)
    {
        if (isPressed) return;
        isPressed = true;
        _spriteRenderer.sprite = depressedSprite;
        _lineRenderer.startColor = Color.green;
        _lineRenderer.endColor = Color.green;
        onPressed.Invoke();
    }

    public override void OnExitCollisionWithEntity(PuzzleEntity other)
    {
        if (!isPressed) return;
        isPressed = false;
        _spriteRenderer.sprite = raisedSprite;
        _lineRenderer.startColor = Color.red;
        _lineRenderer.endColor = Color.red;
        onReleased.Invoke();
    }

    public void SwitchOnVisually(bool on)
    {

        if (on == true)
        {
            _spriteRenderer.sprite = depressedSprite;
            _lineRenderer.startColor = Color.green;
            _lineRenderer.endColor = Color.green;
            onPressed.Invoke();
        }

        if (on == false)
        {
            _spriteRenderer.sprite = raisedSprite;
            _lineRenderer.startColor = Color.red;
            _lineRenderer.endColor = Color.red;
            onReleased.Invoke();
        }
    }
}