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
    class CerberusUndoData : UndoData
    {
        public Cerberus cerberus;
        public Vector2Int position;
        public bool inHole;
        public bool onTopOfGoal;
        public bool collisionDisabledAndPentagramDisplayed;

        public CerberusUndoData(Cerberus cerberus, Vector2Int position, bool inHole,
            bool collisionDisabledAndPentagramDisplayed,
            bool onTopOfGoal)
        {
            this.cerberus = cerberus;
            this.position = position;
            this.onTopOfGoal = onTopOfGoal;
            this.inHole = inHole;
            this.collisionDisabledAndPentagramDisplayed = collisionDisabledAndPentagramDisplayed;
        }

        public override void Load()
        {
            cerberus.inHole = inHole;
            if (!inHole)
            {
                cerberus.MoveForUndo(position);
                cerberus.ResetTransformAndSpriteRendererForUndo();
            }

            cerberus.onTopOfGoal = onTopOfGoal;
            cerberus.SetDisableCollsionAndShowPentagramMarker(collisionDisabledAndPentagramDisplayed, false);
        }
    }

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
    public AudioSource walkSFX;
    public AudioSource pushFailSFX;
    public AnimationCurve talkAnimationCurve;

    protected override void Awake()
    {
        base.Awake();
        input = FindObjectOfType<PuzzleGameplayInput>();
        _cerberusSprite = GetComponent<SpriteRenderer>().sprite;
    }

    public override UndoData GetUndoData()
    {
        var undoData = new CerberusUndoData(this, position, inHole,
            collisionDisabledAndPentagramDisplayed: manager.cerberusFormed != isCerberusMajor, onTopOfGoal);
        return undoData;
    }

    public virtual void ProcessMoveInput()
    {
        if (input.resetPressed)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (input.mergeOrSplit && manager.joinAndSplitEnabled)
        {
            if (this is CerberusMajor cerberusMajor)
            {
                manager.wantsToSplit = true;
                DeclareDoneWithMove();
                cerberusMajor.jumpSpaces.Clear();
                cerberusMajor.RenderJumpPath();
            }
            else
            {
                manager.wantsToJoin = true;
                DeclareDoneWithMove();
            }
        }
    }

    public virtual void StartMove()
    {
        doneWithMove = false;
    }

    public void DeclareDoneWithMove()
    {
        doneWithMove = true;
    }

    // Common movement methods

    public void BasicMove(Vector2Int offset)
    {
        var coord = position + offset;
        var newCell = puzzle.GetCell(coord);
        var collidesWithAndCannotPushEntity = CollidesWithAny(newCell.GetEntitesThatCannotBePushedByStandardMove());
        var blocked = CollidesWith(newCell.floorTile) || collidesWithAndCannotPushEntity;
        if (collidesWithAndCannotPushEntity)
        {
            PlaySfxIfNotPlaying(pushFailSFX);
        }
        else if (!blocked)
        {
            var pushableEntity = newCell.GetEntityPushableByStandardMove();
            if (!pushableEntity)
            {
                puzzle.PushToUndoStack();
                // Move one space
                PlaySfx(walkSFX);
                PlayAnimation(SlideToDestination(coord, AnimationUtility.basicMoveAndPushSpeed));
                Move(coord);
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
                    puzzle.PushToUndoStack();

                    pushableEntity.onStandardPushed.Invoke();

                    PlaySfx(walkSFX);
                    pushableEntity.PlayAnimation(
                        pushableEntity.SlideToDestination(pushCoord, AnimationUtility.basicMoveAndPushSpeed));
                    PlaySfx(pushableEntity.pushedSfx);
                    PlayAnimation(SlideToDestination(coord, AnimationUtility.basicMoveAndPushSpeed));

                    pushableEntity.Move(pushCoord);
                    Move(coord);
                    DeclareDoneWithMove();
                }
                else
                {
                    PlaySfxIfNotPlaying(pushFailSFX);
                    // TODO Make a little bump animation.
                }
            }
        }
    }

    public void SetDisableCollsionAndShowPentagramMarker(bool disableAndShowPentagram, bool invokeCallbacks = true)
    {
        SetCollisionsEnabled(!disableAndShowPentagram, invokeCallbacks);
        pushableByStandardMove = !disableAndShowPentagram;
        pushableByFireball = !disableAndShowPentagram;
        pushableByJacksMultiPush = !disableAndShowPentagram;
        pushableByJacksSuperPush = !disableAndShowPentagram;
        pullable = !disableAndShowPentagram;
        jumpable = !disableAndShowPentagram;
        landableScore = disableAndShowPentagram ? 0 : -1;
        GetComponent<SpriteRenderer>().sprite = disableAndShowPentagram ? pentagramMarker : _cerberusSprite;
    }
}