using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cerberus : PuzzleEntity
{
    public Cerberus()
    {
        isPlayer = true;
        stopsPlayer = true;
        stopsBlock = true;
        pushable = true;
        pushableByFireball = true;
    }

    [HideInInspector] public bool doneWithMove;
    [HideInInspector, ShowInTileInspector] public bool onTopOfGoal;

    [ShowInTileInspector] protected bool isCerberusMajor = false;
    protected PuzzleGameplayInput input;

    private Sprite _cerberusSprite;
    public Sprite pentagramMarker;

    protected override void Awake()
    {
        base.Awake();
        input = FindObjectOfType<PuzzleGameplayInput>();
        _cerberusSprite = GetComponent<SpriteRenderer>().sprite;
    }

    public virtual void ProcessMoveInput()
    {
        if (input.skipMove)
        {
            DeclareDoneWithMove();
        }

        if (input.mergeOrSplit)
        {
            if (isCerberusMajor)
            {
                manager.wantsToSplit = true;
                DeclareDoneWithMove();
            }
            else
            {
                manager.wantsToJoin = true;
                DeclareDoneWithMove();
            }
        }
    }

    public void StartMove()
    {
        doneWithMove = false;
    }

    public void DeclareDoneWithMove()
    {
        doneWithMove = true;
        input.ClearInput();
    }

    // Common movement methods

    protected void BasicMove(Vector2Int offset)
    {
        var coord = position + offset;
        var newCell = puzzle.GetCell(coord);
        var blocked = CollidesWith(newCell.floorTile) || CollidesWithAny(newCell.GetStaticEntities());
        if (!blocked)
        {
            var pushableEntity = newCell.GetPushableEntity();
            if (!pushableEntity)
            {
                puzzle.PushToUndoStack();
                Move(coord);
                DeclareDoneWithMove();
            }
            else
            {
                // Push entity one space
                var pushCoord = pushableEntity.position + offset;
                var pushEntityNewCell = puzzle.GetCell(pushCoord);
                var pushBlocked = pushableEntity.CollidesWith(pushEntityNewCell.floorTile) ||
                                  pushableEntity.CollidesWithAny(pushEntityNewCell.puzzleEntities);
                if (!pushBlocked)
                {
                    puzzle.PushToUndoStack();
                    pushableEntity.Move(pushCoord);
                    Move(coord);
                    DeclareDoneWithMove();
                }
            }
        }
    }

    public void SetDisableCollsionAndShowPentagramMarker(bool disableAndShowPentagram)
    {
        SetCollisionsEnabled(!disableAndShowPentagram);
        pushable = !disableAndShowPentagram;
        landable = disableAndShowPentagram;
        GetComponent<SpriteRenderer>().sprite = disableAndShowPentagram ? pentagramMarker : _cerberusSprite;
    }
}