using UnityEngine;


public class Laguna : Cerberus
{
    public Laguna()
    {
        entityRules = "Laguna can pull objects.";
    }

    public override void ProcessMoveInput()
    {
        base.ProcessMoveInput();
        if (input.specialHeld || input.rightClicked)
        {
            if (input.upPressed || (input.clickedCell.x == position.x && input.clickedCell.y > position.y))
            {
                PullMove(Vector2Int.up);
            }

            else if (input.downPressed || (input.clickedCell.x == position.x && input.clickedCell.y < position.y))
            {
                PullMove(Vector2Int.down);
            }

            else if (input.rightPressed || (input.clickedCell.y == position.y && input.clickedCell.x > position.x))
            {
                PullMove(Vector2Int.right);
            }

            else if (input.leftPressed || (input.clickedCell.y == position.y && input.clickedCell.x < position.x))
            {
                PullMove(Vector2Int.left);
            }
        }
        else
        {
            if (input.upPressed || (input.clickedCell.x == position.x && input.clickedCell.y > position.y &&
                                    input.leftClicked))
            {
                BasicMove(Vector2Int.up);
            }

            else if (input.downPressed || (input.clickedCell.x == position.x && input.clickedCell.y < position.y &&
                                           input.leftClicked))
            {
                BasicMove(Vector2Int.down);
            }

            else if (input.rightPressed || (input.clickedCell.y == position.y && input.clickedCell.x > position.x &&
                                            input.leftClicked))
            {
                BasicMove(Vector2Int.right);
            }

            else if (input.leftPressed || (input.clickedCell.y == position.y && input.clickedCell.x < position.x &&
                                           input.leftClicked))
            {
                BasicMove(Vector2Int.left);
            }
        }

        if (input.cycleCharacter)
        {
            manager.wantsToCycleCharacter = true;
        }

        if (input.undoPressed)
        {
            manager.wantsToUndo = true;
        }
    }


    private void PullMove(Vector2Int offset)
    {
        var coord = position + offset;
        var pullCoord = position - offset;
        var newCell = puzzle.GetCell(coord);
        var pullCell = puzzle.GetCell(pullCoord);
        var blocked = CollidesWith(newCell.floorTile) ||
                      CollidesWithAny(newCell.GetEntitesThatCannotBePushedByStandardMove());
        if (!blocked)
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
                        DeclareDoneWithMove();
                    }
                }
            }
        }
    }
}