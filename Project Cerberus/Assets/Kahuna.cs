using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kahuna : Cerberus
{
    Vector2Int aim = Vector2Int.zero;

    public override void ProcessMoveInput()
    {
        base.ProcessMoveInput();
        if (input.specialHeld)
        {
            if (input.upPressed)
            {
                aim = Vector2Int.up;
            }

            else if (input.downPressed)
            {
                aim = Vector2Int.down;
            }

            else if (input.rightPressed)
            {
                aim = Vector2Int.right;
            }

            else if (input.leftPressed)
            {
                aim = Vector2Int.left;
            }
        }
        else if (input.specialReleased)
        {
            FireBall(aim);
        }
        else
        {
            if (input.upPressed)
            {
                BasicMove(Vector2Int.up);
            }

            else if (input.downPressed)
            {
                BasicMove(Vector2Int.down);
            }

            else if (input.rightPressed)
            {
                BasicMove(Vector2Int.right);
            }

            else if (input.leftPressed)
            {
                BasicMove(Vector2Int.left);
            }
        }
    }

    private void FireBall(Vector2Int offset)
    {
        // Search for pushable block
        var searchCoord = position + offset;
        var searchCell = _puzzle.GetCell(searchCoord);
        PuzzleEntity entityToPush = null;
        while (true)
        {
            if (searchCell.floorTile.stopsFireball)
            {
                break;
            }

            foreach (var entity in searchCell.puzzleEntities)
            {
                if (entity.stopsFireball)
                {
                    if (entity.pushable)
                    {
                        entityToPush = entity;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            searchCoord += offset;
            searchCell = _puzzle.GetCell(searchCoord);
        }

        if (entityToPush != null)
        {
            // Push entity in front of Laguna one space
            var pushCoord = entityToPush.position + offset;
            var pushEntityNewCell = _puzzle.GetCell(pushCoord);
            var pushBlocked = entityToPush.CollidesWith(pushEntityNewCell.floorTile) ||
                              entityToPush.CollidesWithAny(pushEntityNewCell.puzzleEntities);
            if (!pushBlocked)
            {
                entityToPush.Move(pushCoord);
                DeclareDoneWithMove();
            }
        }
    }
}