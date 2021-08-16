using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kahuna : Cerberus
{
    [SerializeField] private GameObject _fireArrow;
    Vector2Int aim = Vector2Int.zero;
    private static int _fireballRange = 32;
    private bool specialActive;

    protected override void Awake()
    {
        base.Awake();
        _fireArrow.SetActive(false);
    }
    public override void ProcessMoveInput()
    {
        base.ProcessMoveInput();
        _fireArrow.SetActive(false);
        var wantsToFire = false;
        if (input.specialPressed)
        {
            aim = Vector2Int.zero;
            specialActive = true;
        }

        if (input.specialReleased)
        {
            specialActive = false;
            wantsToFire = true;
        }

        if (specialActive)
        {
            _fireArrow.SetActive(aim != Vector2Int.zero);
            if (input.upPressed)
            {
                _fireArrow.transform.eulerAngles = new Vector3(0, 0, 90);
                aim = Vector2Int.up;
            }

            else if (input.downPressed)
            {
                _fireArrow.transform.eulerAngles = new Vector3(0, 0, 270);
                aim = Vector2Int.down;
            }

            else if (input.rightPressed)
            {
                _fireArrow.transform.eulerAngles = new Vector3(0, 0, 0);
                aim = Vector2Int.right;
            }

            else if (input.leftPressed)
            {
                _fireArrow.transform.eulerAngles = new Vector3(0, 0, 180);
                aim = Vector2Int.left;
            }
        }
        else if (wantsToFire)
        {
            if (aim != Vector2Int.zero)
            {
                FireBall(aim);
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

    private void FireBall(Vector2Int offset)
    {
        // Search for pushable block
        var searchCoord = position + offset;
        var searchCell = puzzle.GetCell(searchCoord);
        PuzzleEntity entityToPush = null;
        var range = _fireballRange;
        while (range > 0)
        {
            if (searchCell.floorTile.stopsFireball)
            {
                goto AfterWhile;
            }

            foreach (var entity in searchCell.puzzleEntities)
            {
                if (entity.pushableByFireball)
                {
                    entityToPush = entity;
                    goto AfterWhile;
                }
            }

            searchCoord += offset;
            searchCell = puzzle.GetCell(searchCoord);
            range -= 1;
        }

        AfterWhile:
        if (entityToPush != null)
        {
            // Push entity in front of Laguna one space
            var pushCoord = entityToPush.position + offset;
            var pushEntityNewCell = puzzle.GetCell(pushCoord);
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