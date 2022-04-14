using UnityEngine;


public class Laguna : Cerberus
{
    public Laguna()
    {
        entityRules = "Laguna can pull objects.";
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
        base.InterpretCommand(command);
        if (command.specialUp)
        {
            PullMove(Vector2Int.up);
        }
        else if (command.specialDown)
        {
            PullMove(Vector2Int.down);
        }
        else if (command.specialRight)
        {
            PullMove(Vector2Int.right);
        }
        else if (command.specialLeft)
        {
            PullMove(Vector2Int.left);
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

    private void PullMove(Vector2Int offset)
    {
        var coord = position + offset;
        var pullCoord = position - offset;
        var newCell = puzzle.GetCell(coord);
        var pullCell = puzzle.GetCell(pullCoord);
        var collidesWithAndCannotPushEntity = CollidesWithAny(newCell.GetEntitesThatCannotBePushedByStandardMove());
        var blocked = CollidesWith(newCell.floorTile) || collidesWithAndCannotPushEntity;
        if (collidesWithAndCannotPushEntity)
        {
            PlaySfxIfNotPlaying(pushFailSFX);
        }
        else if (!blocked)
        {
            var pushableEntity = newCell.GetEntityPushableByStandardMove();
            var entityToPull = pullCell.GetPullableEntity();
            // Pull move fails with no entity to pull
            if (entityToPull)
            {
                if (!pushableEntity)
                {
                    puzzle.PushToUndoStack();
                    var p = position;

                    entityToPull.onPulled.Invoke();

                    PlayAnimation(SlideToDestination(coord, AnimationUtility.basicMoveAndPushSpeed));
                    entityToPull.PlayAnimation(entityToPull.SlideToDestination(p,
                        AnimationUtility.basicMoveAndPushSpeed));
                    PlaySfx(walkSFX);
                    PlaySfx(entityToPull.pushedSfx);

                    Move(coord);
                    entityToPull.Move(p);
                    
                    hasPerformedSpecial = true;
                    DeclareDoneWithMove();
                }
                else
                {
                    // Push entity in front of Laguna one space and pull block behind
                    var pushCoord = pushableEntity.position + offset;
                    var pushEntityNewCell = puzzle.GetCell(pushCoord);
                    var pushBlocked = pushableEntity.CollidesWith(pushEntityNewCell.floorTile) ||
                                      pushableEntity.CollidesWithAny(pushEntityNewCell.puzzleEntities);
                    if (!pushBlocked)
                    {
                        puzzle.PushToUndoStack();
                        var p = position;

                        pushableEntity.onStandardPushed.Invoke();
                        entityToPull.onPulled.Invoke();

                        PlayAnimation(SlideToDestination(coord, AnimationUtility.basicMoveAndPushSpeed));
                        pushableEntity.PlayAnimation(
                            pushableEntity.SlideToDestination(pushCoord, AnimationUtility.basicMoveAndPushSpeed));
                        entityToPull.PlayAnimation(
                            entityToPull.SlideToDestination(p, AnimationUtility.basicMoveAndPushSpeed));
                        PlaySfx(walkSFX);
                        PlaySfx(pushableEntity.pushedSfx);
                        PlaySfx(entityToPull.pushedSfx);

                        pushableEntity.Move(pushCoord);
                        Move(coord);
                        entityToPull.Move(p);
                        
                        hasPerformedSpecial = true;
                        DeclareDoneWithMove();
                    }
                    else
                    {
                        PlaySfxIfNotPlaying(pushFailSFX);
                    }
                }
            }
        }
    }
}