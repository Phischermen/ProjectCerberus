﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Kahuna : Cerberus
{
    [SerializeField] private GameObject fireArrow;
    [SerializeField] private GameObject fireBall;
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
        if (input.specialPressed)
        {
            aim = Vector2Int.zero;
            _specialActive = true;
        }

        if (input.specialReleased && _specialActive)
        {
            _specialActive = false;
            wantsToFire = true;
        }

        if (_specialActive)
        {
            fireArrow.SetActive(aim != Vector2Int.zero);
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

        if (input.cycleCharacter)
        {
            _specialActive = false;
            fireArrow.SetActive(false);
            manager.wantsToCycleCharacter = true;
        }

        if (input.undoPressed)
        {
            if (aim != Vector2Int.zero)
            {
                aim = Vector2Int.zero;
            }
            else
            {
                manager.wantsToUndo = true;
            }
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
                    // Get time to reach target
                    var startingPosition = transform.position;
                    var destinationPosition = puzzle.GetCellCenterWorld(searchCoord - offset);
                    var distanceToTravel = Vector3.Distance(startingPosition, destinationPosition);
                    var time = GetTimeToHitTarget(AnimationUtility.initialFireballSpeed,
                        AnimationUtility.fireBallAcceleration, distanceToTravel);
                    entityToPushOrInteractWith.PlayAnimation(
                        entityToPushOrInteractWith.SlideToDestination(pushCoord,
                            AnimationUtility.basicMoveAndPushSpeed, time));
                    PlayAnimation(LaunchFireball(destinationPosition, AnimationUtility.initialFireballSpeed,
                        AnimationUtility.fireBallAcceleration, distanceToTravel));
                    DeclareDoneWithMove();
                }
            }

            entityToPushOrInteractWith.onHitByFireball.Invoke();
        }
    }

    // Animation helper
    float GetTimeToHitTarget(float initialSpeed, float acceleration, float distance)
    {
        // 𝅘𝅥𝅯 Negative b plus or minus radical! b squared minus 4ac. All over 2a! 𝅘𝅥𝅯
        return (-initialSpeed + Mathf.Sqrt((initialSpeed * initialSpeed) + (2f * acceleration * distance))) /
               (acceleration);
    }

    // Animation

    public IEnumerator LaunchFireball(Vector3 destinationPosition, float initialSpeed, float acceleration,
        float distanceToTravel)
    {
        var startingPosition = transform.position;
        var distanceTraveled = 0f;
        var speed = initialSpeed;

        fireBall.transform.position = startingPosition;
        while (distanceTraveled < distanceToTravel && animationMustStop == false)
        {
            // Increment speed
            speed += acceleration * Time.deltaTime;
            // Increment distance travelled
            var delta = speed * Time.deltaTime;
            distanceTraveled += delta;
            // Set position
            var interpolation = distanceTraveled / distanceToTravel;
            fireBall.transform.position = Vector3.Lerp(startingPosition, destinationPosition, interpolation);
            yield return new WaitForFixedUpdate();
        }

        fireBall.transform.position = destinationPosition;
        animationIsRunning = false;
        animationMustStop = false;
    }
}