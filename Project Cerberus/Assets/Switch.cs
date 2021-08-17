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
    Switch()
    {
        isStatic = true;
        landable = true;
    }

    public override void OnEnterCollisionWithEntity(PuzzleEntity other)
    {
        if (isPressed) return;
        isPressed = true;
        GetComponent<SpriteRenderer>().sprite = depressedSprite;
        onPressed.Invoke();
    }

    public override void OnExitCollisionWithEntity(PuzzleEntity other)
    {
        if (!isPressed) return;
        isPressed = false;
        GetComponent<SpriteRenderer>().sprite = raisedSprite;
        onReleased.Invoke();
    }
}
