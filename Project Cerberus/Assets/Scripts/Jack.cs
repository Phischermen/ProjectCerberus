using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jack : Cerberus
{
    private static int _superPushRange = 32;

    public AudioSource _superPushSFX;

    public Jack()
    {
        entityRules =
            "Jack can push a single object really far, but can't push a row of objects farther than one tile.";
    }

    public override void CheckInputForResetUndoOrCycle()
    {
        base.CheckInputForResetUndoOrCycle();
        if (input.cycleCharacter)
        {
            manager.wantsToCycleCharacter = true;
        }

        if (input.undoPressed)
        {
            manager.wantsToUndo = true;
        }
    }

    public override CerberusCommand ProcessInputIntoCommand()
    {
        var command = base.ProcessInputIntoCommand();
        if (input.specialHeld || input.rightClicked)
        {
            if (input.upPressed || (input.clickedCell.x == position.x && input.clickedCell.y > position.y))
            {
                command.specialUp = true;
            }

            else if (input.downPressed || (input.clickedCell.x == position.x && input.clickedCell.y < position.y))
            {
                command.specialDown = true;
            }

            else if (input.rightPressed || (input.clickedCell.y == position.y && input.clickedCell.x > position.x))
            {
                command.specialRight = true;
            }

            else if (input.leftPressed || (input.clickedCell.y == position.y && input.clickedCell.x < position.x))
            {
                command.specialLeft = true;
            }
        }
        else
        {
            if (input.upPressed || (input.clickedCell.x == position.x && input.clickedCell.y > position.y &&
                                    input.leftClicked))
            {
                command.moveUp = true;
            }

            else if (input.downPressed || (input.clickedCell.x == position.x && input.clickedCell.y < position.y &&
                                           input.leftClicked))
            {
                command.moveDown = true;
            }

            else if (input.rightPressed || (input.clickedCell.y == position.y && input.clickedCell.x > position.x &&
                                            input.leftClicked))
            {
                command.moveRight = true;
            }

            else if (input.leftPressed || (input.clickedCell.y == position.y && input.clickedCell.x < position.x &&
                                           input.leftClicked))
            {
                command.moveLeft = true;
            }
        }

        return command;
    }

    public override void InterpretCommand(CerberusCommand command)
    {
        base.InterpretCommand(command);
        if (command.specialUp)
        {
            SuperPushMove(Vector2Int.up);
        }
        else if (command.specialDown)
        {
            SuperPushMove(Vector2Int.down);
        }
        else if (command.specialRight)
        {
            SuperPushMove(Vector2Int.right);
        }
        else if (command.specialLeft)
        {
            SuperPushMove(Vector2Int.left);
        }
        else if (command.moveUp)
        {
            BasicMove(Vector2Int.up);
        }
        else if (command.moveDown)
        {
            BasicMove(Vector2Int.down);
        }
        else if (command.moveRight)
        {
            BasicMove(Vector2Int.right);
        }
        else if (command.moveLeft)
        {
            BasicMove(Vector2Int.left);
        }
    }

    private void SuperPushMove(Vector2Int offset)
    {
        var coord = position + offset;
        var newCell = puzzle.GetCell(coord);
        var collidesWithAndCannotPushEntity = CollidesWithAny(newCell.GetEntitesThatCannotBePushedByStandardMove());
        var blocked = CollidesWith(newCell.floorTile) || collidesWithAndCannotPushEntity;
        var entitiesToPush = GetEntitiesToPush(offset);
        if (collidesWithAndCannotPushEntity)
        {
            PlaySfxIfNotPlaying(pushFailSFX);
        }
        else if (!blocked && entitiesToPush.Count == 0)
        {
            puzzle.PushToUndoStack();
            PlaySfxPitchShift(walkSFX, 0.9f, 1.1f);
            PlayAnimation(SlideToDestination(coord, AnimationUtility.basicMoveAndPushSpeed));
            Move(coord);
            onStandardMove.Invoke();
            DeclareDoneWithMove();
        }
        else if (entitiesToPush.Count == 1)
        {
            // Push entity until it hits a wall
            var pushableEntity = entitiesToPush[0];
            var searchPosition = pushableEntity.position + offset;
            var range = _superPushRange;
            var distancePushed = 0;
            pushableEntity.isSuperPushed = true;
            while (range > 0)
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

                distancePushed += 1;
                range -= 1;
            }

            // Move across searched tiles.
            puzzle.PushToUndoStack();

            pushableEntity.onSuperPushed.Invoke();

            PlaySfx(_superPushSFX);
            pushableEntity.PlayAnimation(
                pushableEntity.SlideToDestination(searchPosition, AnimationUtility.superPushAnimationSpeed));
            pushableEntity.PlaySfx(pushableEntity.superPushedSfx);

            for (int i = 0; i < distancePushed; i++)
            {
                var firstMove = i == 0;
                var lastMove = i == distancePushed - 1;
                // The super pushed object "lands" at the last space it moves and "lifts off" at the space it
                // starts.
                pushableEntity.isSuperPushed = !lastMove && !firstMove;
                pushableEntity.Move(pushableEntity.position + offset, !lastMove, lastMove && !firstMove);
            }

            hasPerformedSpecial = true;
            DeclareDoneWithMove();
        }
        else if (!blocked)
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
                puzzle.PushToUndoStack();
                for (var i = entitiesToPush.Count - 1; i >= 0; i--)
                {
                    var entity = entitiesToPush[i];
                    var entityPushCoord = entity.position + offset;

                    entity.onMultiPushed.Invoke();

                    entity.PlayAnimation(entity.SlideToDestination(entityPushCoord,
                        AnimationUtility.basicMoveAndPushSpeed));
                    entity.PlaySfx(entity.superPushedSfx);

                    entity.Move(entityPushCoord);
                }

                PlaySfxPitchShift(superPushedSfx, 0.9f, 1.1f);
                PlayAnimation(SlideToDestination(coord, AnimationUtility.basicMoveAndPushSpeed));
                Move(coord);

                hasPerformedSpecial = true;
                DeclareDoneWithMove();
            }
        }
    }

    private List<PuzzleEntity> GetEntitiesToPush(Vector2Int offset)
    {
        List<PuzzleEntity> entities = new List<PuzzleEntity>();
        var searchPosition = position;
        while (true)
        {
            var searchedCell = puzzle.GetCell(searchPosition + offset);
            var nextPushableEntity = (entities.Count > 0)
                ? searchedCell.GetPushableEntityForMultiPush()
                : searchedCell.GetPushableEntityForSuperPush();
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