using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cerberus : PuzzleEntity
{
    // Start is called before the first frame update
    public Cerberus()
    {
        isPlayer = true;
        stopsPlayer = true;
        stopsBlock = true;
        pushable = true;
    }

    public bool doneWithMove;

    protected PuzzleGameplayInput input;

    // Update is called once per frame
    private void Start()
    {
        input = FindObjectOfType<PuzzleGameplayInput>();
    }

    public virtual void ProcessMoveInput()
    {
        if (input.skipMove)
        {
            DeclareDoneWithMove();
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
        var newCell = _puzzle.GetCell(coord);
        var blocked = CollidesWith(newCell.floorTile) || CollidesWithAny(newCell.GetStaticEntities());
        if (!blocked)
        {
            var pushableEntity = newCell.GetPushableEntity();
            if (!pushableEntity)
            {
                Move(coord);
                DeclareDoneWithMove();
            }
            else
            {
                // Push entity one space
                var pushCoord = pushableEntity.position + offset;
                var pushEntityNewCell = _puzzle.GetCell(pushCoord);
                var pushBlocked = pushableEntity.CollidesWith(pushEntityNewCell.floorTile) ||
                                  pushableEntity.CollidesWithAny(pushEntityNewCell.puzzleEntities);
                if (!pushBlocked)
                {
                    pushableEntity.Move(pushCoord);
                    Move(coord);
                    DeclareDoneWithMove();
                }
            }
        }
    }
}