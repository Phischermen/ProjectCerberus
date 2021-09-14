using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Kahuna : Cerberus
{
    [SerializeField] private GameObject fireArrow;
    [SerializeField] private AudioSource fireballSFX;

    Vector2Int aim = Vector2Int.zero;
    private static int _fireballRange = 32;
    private bool _specialActive;

    Kahuna()
    {
        entityRules = "Kahuna can fire fireballs, that push or interact with objects.";
    }

    protected override void Awake()
    {
        base.Awake();
        fireArrow.SetActive(false);
    }

    public override void ProcessMoveInput()
    {
        base.ProcessMoveInput();
        fireArrow.SetActive(false);
        var wantsToFire = false;
        if (input.specialPressed && !_specialActive)
        {
            aim = Vector2Int.left;
            _specialActive = true;
        }
        else if (input.specialPressed && _specialActive)
        {
            _specialActive = false;
            wantsToFire = true;
        }

        if (_specialActive)
        {
            fireArrow.SetActive(true);
            if (input.upPressed)
            {
                fireArrow.transform.eulerAngles = new Vector3(0, 0, 90);
                aim = Vector2Int.up;
            }

            else if (input.downPressed)
            {
                fireArrow.transform.eulerAngles = new Vector3(0, 0, 270);
                aim = Vector2Int.down;
            }

            else if (input.rightPressed)
            {
                fireArrow.transform.eulerAngles = new Vector3(0, 0, 0);
                aim = Vector2Int.right;
            }

            else if (input.leftPressed)
            {
                fireArrow.transform.eulerAngles = new Vector3(0, 0, 180);
                aim = Vector2Int.left;
            }

            if (input.backOutOfAbility)
            {
                _specialActive = false;
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

            ProcessUndoMergeSplitSkipInput();
        }
    }

    private void FireBall(Vector2Int offset)
    {
        // Search for pushable block
        var searchCoord = position + offset;
        var searchCell = puzzle.GetCell(searchCoord);
        PuzzleEntity entityToPushOrInteractWith = null;
        var range = _fireballRange;
        while (range > 0)
        {
            if (searchCell.floorTile.stopsFireball)
            {
                goto AfterWhile;
            }

            foreach (var entity in searchCell.puzzleEntities)
            {
                if (entity.pushableByFireball || entity.interactsWithFireball)
                {
                    entityToPushOrInteractWith = entity;
                    goto AfterWhile;
                }
            }

            searchCoord += offset;
            searchCell = puzzle.GetCell(searchCoord);
            range -= 1;
        }

        AfterWhile:
        if (entityToPushOrInteractWith != null)
        {
            puzzle.PushToUndoStack();
            PlaySfxPitchShift(fireballSFX, 0.9f, 1.1f);
            // Push or interact with entity
            if (entityToPushOrInteractWith.interactsWithFireball)
            {
                // Interact with entity
                entityToPushOrInteractWith.OnShotByKahuna();
                DeclareDoneWithMove();
            }
            else
            {
                // Push entity in front of Kahuna one space
                var pushCoord = entityToPushOrInteractWith.position + offset;
                var pushEntityNewCell = puzzle.GetCell(pushCoord);
                var pushBlocked = entityToPushOrInteractWith.CollidesWith(pushEntityNewCell.floorTile) ||
                                  entityToPushOrInteractWith.CollidesWithAny(pushEntityNewCell.puzzleEntities);
                if (!pushBlocked)
                {
                    entityToPushOrInteractWith.Move(pushCoord);
                    entityToPushOrInteractWith.PlaySfx(entityToPushOrInteractWith.pushedByFireballSfx);
                    entityToPushOrInteractWith.PlayAnimation(
                        entityToPushOrInteractWith.SlideToDestination(pushCoord,
                            AnimationUtility.basicMoveAndPushSpeed));
                    DeclareDoneWithMove();
                }
            }
        }
    }
}