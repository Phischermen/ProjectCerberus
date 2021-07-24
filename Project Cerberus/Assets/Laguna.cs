using UnityEngine;


public class Laguna : Cerberus
{
    public override void ProcessMoveInput()
    {
        base.ProcessMoveInput();
        if (Input.GetButton("Fire1"))
        {
            if (_verticalAxisJustHeld == 1)
            {
                PullMove(Vector2Int.up);
            }

            if (_verticalAxisJustHeld == -1)
            {
                PullMove(Vector2Int.down);
            }

            if (_horizontalAxisJustHeld == 1)
            {
                PullMove(Vector2Int.right);
            }

            if (_horizontalAxisJustHeld == -1)
            {
                PullMove(Vector2Int.left);
            }
        }
        else
        {
            if (_verticalAxisJustHeld == 1)
            {
                BasicLagunaMove(Vector2Int.up);
            }

            if (_verticalAxisJustHeld == -1)
            {
                BasicLagunaMove(Vector2Int.down);
            }

            if (_horizontalAxisJustHeld == 1)
            {
                BasicLagunaMove(Vector2Int.right);
            }

            if (_horizontalAxisJustHeld == -1)
            {
                BasicLagunaMove(Vector2Int.left);
            }
        }
    }

    private void BasicLagunaMove(Vector2Int offset)
    {
        var coord = position + offset;
        var newCell = _puzzle.GetCell(coord);
        if (!CollidesWith(newCell.floorTile))
        {
            var pushableEntity = _puzzle.GetPushableEntity(coord);
            if (!pushableEntity)
            {
                Move(coord);
            }
            else
            {
                // Push entity one space
                var pushCoord = pushableEntity.position + offset;
                var pushEntityNewCell = _puzzle.GetCell(pushCoord);
                if (!pushableEntity.CollidesWith(pushEntityNewCell.floorTile))
                {
                    var pushBlockedByEntity = false;
                    foreach (var entity in pushEntityNewCell.puzzleEntities)
                    {
                        if (pushableEntity.CollidesWith(entity))
                        {
                            pushBlockedByEntity = true;
                            break;
                        }
                    }

                    if (!pushBlockedByEntity)
                    {
                        pushableEntity.Move(pushCoord);
                        Move(coord);
                    }
                }
            }
        }
    }

    private void PullMove(Vector2Int offset)
    {
        var coord = position + offset;
        var pullCoord = position - offset;
        var newCell = _puzzle.GetCell(coord);
        if (!CollidesWith(newCell.floorTile))
        {
            var entityToPull = _puzzle.GetPushableEntity(pullCoord);
            var pushableEntity = _puzzle.GetPushableEntity(coord);
            if (!pushableEntity)
            {
                entityToPull?.Move(position);
                Move(coord);
            }
            else
            {
                // Push entity in front of Laguna one space
                var pushCoord = pushableEntity.position + offset;
                var pushEntityNewCell = _puzzle.GetCell(pushCoord);
                if (!pushableEntity.CollidesWith(pushEntityNewCell.floorTile))
                {
                    var pushBlockedByEntity = false;
                    foreach (var entity in pushEntityNewCell.puzzleEntities)
                    {
                        if (pushableEntity.CollidesWith(entity))
                        {
                            pushBlockedByEntity = true;
                            break;
                        }
                    }

                    if (!pushBlockedByEntity)
                    {
                        pushableEntity.Move(pushCoord);
                        entityToPull?.Move(position);
                        Move(coord);
                    }
                }
            }
        }
    }
}