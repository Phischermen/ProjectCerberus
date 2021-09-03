using UnityEngine;


public class Laguna : Cerberus
{
    public override void ProcessMoveInput()
    {
        base.ProcessMoveInput();
        if (input.specialHeld)
        {
            if (input.upPressed)
            {
                PullMove(Vector2Int.up);
            }

            else if (input.downPressed)
            {
                PullMove(Vector2Int.down);
            }

            else if (input.rightPressed)
            {
                PullMove(Vector2Int.right);
            }

            else if (input.leftPressed)
            {
                PullMove(Vector2Int.left);
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

        if (input.cycleCharacter)
        {
            manager.wantsToCycleCharacter = true;
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
                    var p = position;
                    Move(coord);
                    entityToPull.Move(p);
                    PlayAnimation(SlideToDestination(coord, AnimationConstants.basicMoveAndPushSpeed));
                    entityToPull.PlayAnimation(entityToPull.SlideToDestination(p,
                        AnimationConstants.basicMoveAndPushSpeed));
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
                        var p = position;
                        pushableEntity.Move(pushCoord);
                        Move(coord);
                        entityToPull.Move(p);
                        PlayAnimation(SlideToDestination(coord, AnimationConstants.basicMoveAndPushSpeed));
                        pushableEntity.PlayAnimation(
                            pushableEntity.SlideToDestination(pushCoord, AnimationConstants.basicMoveAndPushSpeed));
                        entityToPull.PlayAnimation(
                            entityToPull.SlideToDestination(p, AnimationConstants.basicMoveAndPushSpeed));
                        DeclareDoneWithMove();
                    }
                }
            }
        }
    }
}