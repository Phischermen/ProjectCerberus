using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Switch : PuzzleEntity
{
    public UnityEvent onPressed;
    public UnityEvent onReleased;

    [HideInInspector, ShowInTileInspector] public bool isPressed;
    [SerializeField] private Sprite depressedSprite;
    [SerializeField] private Sprite raisedSprite;
    private SpriteRenderer _spriteRenderer;
    private LineRenderer _lineRenderer;

    Switch()
    {
        entityRules = "Switches control other objects. A switch must be held down with an object.";
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
}