using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Cerberus : PuzzleEntity
{
    public Cerberus()
    {
        isPlayer = true;
        stopsPlayer = true;
        stopsBlock = true;
        pullable = true;
        pushableByStandardMove = true;
        pushableByJacksSuperPush = true;
        pushableByJacksMultiPush = true;
        pushableByFireball = true;
        jumpable = true;
    }

    [HideInInspector] public bool doneWithMove;
    [HideInInspector, ShowInTileInspector] public bool onTopOfGoal;

    [ShowInTileInspector] public bool isCerberusMajor = false;
    protected PuzzleGameplayInput input;

    private Sprite _cerberusSprite;
    public Sprite pentagramMarker;
    [FormerlySerializedAs("_walkSFX")] public AudioSource walkSFX;

    protected override void Awake()
    {
        base.Awake();
        input = FindObjectOfType<PuzzleGameplayInput>();
        _cerberusSprite = GetComponent<SpriteRenderer>().sprite;
    }

    public virtual void ProcessMoveInput()
    {
        if (input.resetPressed)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (input.skipMove)
        {
            DeclareDoneWithMove();
        }

        if (input.mergeOrSplit && manager.joinAndSplitEnabled)
        {
            if (isCerberusMajor)
            {
                manager.wantsToSplit = true;
                DeclareDoneWithMove();
            }
            else
            {
                manager.wantsToJoin = true;
                DeclareDoneWithMove();
            }
        }
    }

    public void StartMove()
    {
        doneWithMove = false;
    }

    public void DeclareDoneWithMove()
    {
        doneWithMove = true;
        input.ClearInput();
    }

    // Common movement methods
    protected void BasicMove(Vector2Int offset)
    {
        var coord = position + offset;
        var newCell = puzzle.GetCell(coord);
        var blocked = CollidesWith(newCell.floorTile) ||
                      CollidesWithAny(newCell.GetEntitesThatCannotBePushedByStandardMove());
        if (!blocked)
        {
            var pushableEntity = newCell.GetEntityPushableByStandardMove();
            if (!pushableEntity)
            {
                // Move one space
                Move(coord);
                PlaySfx(walkSFX);
                PlayAnimation(SlideToDestination(coord, AnimationUtility.basicMoveAndPushSpeed));
                DeclareDoneWithMove();
            }
            else
            {
                // Push entity one space
                var pushCoord = pushableEntity.position + offset;
                var pushEntityNewCell = puzzle.GetCell(pushCoord);
                var pushBlocked = pushableEntity.CollidesWith(pushEntityNewCell.floorTile) ||
                                  pushableEntity.CollidesWithAny(pushEntityNewCell.puzzleEntities);
                if (!pushBlocked)
                {
                    pushableEntity.Move(pushCoord);
                    Move(coord);
                    
                    pushableEntity.onStandardPushed.Invoke();
                    
                    PlaySfx(walkSFX);
                    pushableEntity.PlayAnimation(
                        pushableEntity.SlideToDestination(pushCoord, AnimationUtility.basicMoveAndPushSpeed));
                    PlaySfx(pushableEntity.pushedSfx);
                    PlayAnimation(SlideToDestination(coord, AnimationUtility.basicMoveAndPushSpeed));
                    DeclareDoneWithMove();
                }
            }
        }
    }

    public void SetDisableCollsionAndShowPentagramMarker(bool disableAndShowPentagram)
    {
        SetCollisionsEnabled(!disableAndShowPentagram);
        pushableByStandardMove = !disableAndShowPentagram;
        pushableByFireball = !disableAndShowPentagram;
        pushableByJacksMultiPush = !disableAndShowPentagram;
        pushableByJacksSuperPush = !disableAndShowPentagram;
        landable = disableAndShowPentagram;
        GetComponent<SpriteRenderer>().sprite = disableAndShowPentagram ? pentagramMarker : _cerberusSprite;
    }

}