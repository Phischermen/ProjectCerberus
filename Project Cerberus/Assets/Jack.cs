using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jack : Cerberus
{
    public override void ProcessMoveInput()
    {
        base.ProcessMoveInput();
        if (input.specialHeld)
        {
            if (input.upPressed)
            {
                SuperPushMove(Vector2Int.up);
            }

            else if (input.downPressed)
            {
                SuperPushMove(Vector2Int.down);
            }

            else if (input.rightPressed)
            {
                SuperPushMove(Vector2Int.right);
            }

            else if (input.leftPressed)
            {
                SuperPushMove(Vector2Int.left);
            }
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

    private void SuperPushMove(Vector2Int offset)
    {
        var coord = position + offset;
        var newCell = puzzle.GetCell(coord);
        var blocked = CollidesWith(newCell.floorTile) || CollidesWithAny(newCell.GetStaticEntities());
        if (!blocked)
        {
            var entitiesToPush = GetEntitiesToPush(offset);
            if (entitiesToPush.Count == 0)
            {
                Move(coord);
                DeclareDoneWithMove();
            }
            else if (entitiesToPush.Count == 1)
            {
                // Push entity until it hits a wall
                var pushableEntity = entitiesToPush[0];
                var searchPosition = pushableEntity.position + offset;
                while (true)
                {
                    var searchCell = puzzle.GetCell(searchPosition);
                    var pushBlocked = pushableEntity.CollidesWith(searchCell.floorTile) ||
                                      pushableEntity.CollidesWithAny(searchCell.puzzleEntities);
                    if (!pushBlocked)
                    {
                        searchPosition += offset;
                    }
                    else
                    {
                        searchPosition -= offset;
                        break;
                    }
                }

                pushableEntity.Move(searchPosition);
                DeclareDoneWithMove();
            }
            else
            {
                // Check if last entity can move
                var lastPushableEntity = entitiesToPush[entitiesToPush.Count - 1];
                var pushCoord = lastPushableEntity.position + offset;
                var pushEntityNewCell = puzzle.GetCell(pushCoord);
                var pushBlocked = lastPushableEntity.CollidesWith(pushEntityNewCell.floorTile) ||
                                  lastPushableEntity.CollidesWithAny(pushEntityNewCell.GetStaticEntities());
                if (!pushBlocked)
                {
                    // Push each entity in front of Jack once. Iterate backwards to avoid triggering "OnEnter" unnecessarily
                    for (var i = entitiesToPush.Count - 1; i >= 0; i--)
                    {
                        var entity = entitiesToPush[i];
                        entity.Move(entity.position + offset);
                    }

                    Move(coord);
                    DeclareDoneWithMove();
                }
            }
        }
    }

    private List<PuzzleEntity> GetEntitiesToPush(Vector2Int offset)
    {
        List<PuzzleEntity> entities = new List<PuzzleEntity>();
        var searchPosition = position;
        while (true)
        {
            var nextPushableEntity = puzzle.GetCell(searchPosition + offset).GetPushableEntity();
            if (nextPushableEntity == null)
            {
                // Last entity found.
                break;
            }

            entities.Add(nextPushableEntity);
            searchPosition = nextPushableEntity.position;
        }

        return entities;
    }
}