using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.U2D.Animation;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Cerberus : PuzzleEntity
{
    [RuntimeInitializeOnLoadMethod]
    static void OnRuntimeMethodLoad()
    {
        PhotonPeer.RegisterType(typeof(CerberusCommand), (byte) 'C', CerberusCommand.Serialize,
            CerberusCommand.Deserialize);
    }

    public class CerberusCommand
    {
        public byte cerberusId;
        public bool[] commands = new bool[13];

        public bool moveUp
        {
            get => commands[0];
            set => commands[0] = value;
        }

        public bool moveDown
        {
            get => commands[1];
            set => commands[1] = value;
        }

        public bool moveLeft
        {
            get => commands[2];
            set => commands[2] = value;
        }

        public bool moveRight
        {
            get => commands[3];
            set => commands[3] = value;
        }

        public bool specialUp
        {
            get => commands[4];
            set => commands[4] = value;
        }

        public bool specialDown
        {
            get => commands[5];
            set => commands[5] = value;
        }

        public bool specialLeft
        {
            get => commands[6];
            set => commands[6] = value;
        }

        public bool specialRight
        {
            get => commands[7];
            set => commands[7] = value;
        }

        public bool specialActivated
        {
            get => commands[8];
            set => commands[8] = value;
        }

        public bool specialDeactivated
        {
            get => commands[9];
            set => commands[9] = value;
        }

        public bool specialPerformed
        {
            get => commands[10];
            set => commands[10] = value;
        }

        public bool mergeOrSplit
        {
            get => commands[11];
            set => commands[11] = value;
        }

        public bool skipCerberusJumpAnimation
        {
            get => commands[12];
            set => commands[12] = value;
        }

        public static byte[] Serialize(object o)
        {
            var command = (CerberusCommand) o;
            byte moveAndSpecialByte = 0;
            // Serialize move & special
            if (command.specialUp) moveAndSpecialByte += 1;
            if (command.specialDown) moveAndSpecialByte += 2;
            if (command.specialRight) moveAndSpecialByte += 4;
            if (command.specialLeft) moveAndSpecialByte += 8;
            if (command.moveUp) moveAndSpecialByte += 16;
            if (command.moveDown) moveAndSpecialByte += 32;
            if (command.moveRight) moveAndSpecialByte += 64;
            if (command.moveLeft) moveAndSpecialByte += 128;
            // Serialize special status and Id
            byte statusAndId = 0;
            if (command.specialActivated) statusAndId += 1;
            if (command.specialDeactivated) statusAndId += 2;
            if (command.specialPerformed) statusAndId += 4;
            if (command.mergeOrSplit) statusAndId += 8;
            if (command.skipCerberusJumpAnimation) statusAndId += 16;
            if (command.cerberusId == 1) statusAndId += 32;
            if (command.cerberusId == 2) statusAndId += 64;
            if (command.cerberusId == 3) statusAndId += 128;
            return new[] {moveAndSpecialByte, statusAndId};
        }

        public static object Deserialize(byte[] data)
        {
            var command = new CerberusCommand();
            var bitArray = new BitArray(data);

            // Deserialize move & special
            command.specialUp = bitArray[0];
            command.specialDown = bitArray[1];
            command.specialRight = bitArray[2];
            command.specialLeft = bitArray[3];
            command.moveUp = bitArray[4];
            command.moveDown = bitArray[5];
            command.moveRight = bitArray[6];
            command.moveLeft = bitArray[7];

            // Deserialize special status and Id
            command.specialActivated = bitArray[8 + 0];
            command.specialDeactivated = bitArray[8 + 1];
            command.specialPerformed = bitArray[8 + 2];
            command.mergeOrSplit = bitArray[8 + 3];
            command.skipCerberusJumpAnimation = bitArray[8 + 4];
            if (bitArray[8 + 5])
            {
                command.cerberusId = 1;
            }

            if (bitArray[8 + 6])
            {
                command.cerberusId = 2;
            }

            if (bitArray[8 + 7])
            {
                command.cerberusId = 3;
            }
            
            return command;
        }

        public bool doSomething =>
            moveUp ||
            moveDown ||
            moveLeft ||
            moveRight ||
            specialUp ||
            specialDown ||
            specialLeft ||
            specialRight ||
            specialActivated ||
            specialDeactivated ||
            specialPerformed ||
            mergeOrSplit ||
            skipCerberusJumpAnimation;
    }

    class CerberusStateData : StateData
    {
        public Cerberus cerberus;
        public Vector2Int position => myVector2Int;
        public bool inHole => booleans[0];
        public bool onTopOfGoal => booleans[1];
        public bool collisionDisabledAndPentagramDisplayed => booleans[2];
        public bool hasPerformedSpecial => booleans[3];

        public CerberusStateData(Cerberus cerberus, Vector2Int position, bool inHole,
            bool collisionDisabledAndPentagramDisplayed,
            bool onTopOfGoal, bool hasPerformedSpecial)
        {
            this.cerberus = cerberus;
            myVector2Int = position;
            booleans[0] = inHole;
            booleans[1] = onTopOfGoal;
            booleans[2] = collisionDisabledAndPentagramDisplayed;
            booleans[3] = hasPerformedSpecial;
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
            cerberus.hasPerformedSpecial = hasPerformedSpecial;
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
        // Hades must be able to reach Cerberus, so this must be set as 0.
        // CerberusMajor is incapable of landing on itself anyway. 
        landableScore = 0;
    }

    [HideInInspector] public bool doneWithMove;
    [HideInInspector, ShowInTileInspector] public bool onTopOfGoal;
    [ShowInTileInspector] public bool hasPerformedSpecial;
    [ShowInTileInspector] public bool isCerberusMajor;

    protected PuzzleGameplayInput input;
    
    [FormerlySerializedAs("spriteResolver")] public SpriteLibrary spriteLibrary;
    public SpriteResolver spriteResolver;
    public AudioSource walkSFX;
    public AudioSource pushFailSFX;
    public AnimationCurve talkAnimationCurve;

    public UnityEvent onStandardMove;

    protected override void Awake()
    {
        base.Awake();
        input = FindObjectOfType<PuzzleGameplayInput>();
        spriteLibrary = GetComponent<SpriteLibrary>();
        spriteResolver = GetComponent<SpriteResolver>();
    }

    public override StateData GetUndoData()
    {
        var undoData = new CerberusStateData(this, position, inHole,
            collisionDisabledAndPentagramDisplayed: manager.cerberusFormed != isCerberusMajor, onTopOfGoal,
            hasPerformedSpecial);
        return undoData;
    }

    // NOTE: The input checked in the following method is not networked, or is conditionally networked.
    public virtual void CheckInputForResetUndoOrCycle()
    {
        if (input.resetPressed)
        {
            if (PhotonNetwork.InRoom)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    manager.ReplayLevel();
                }
                else
                {
                    // TODO display message that says that only the host may reset the level.
                }
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }

    public virtual CerberusCommand ProcessInputIntoCommand()
    {
        var command = new CerberusCommand();
        if (input.mergeOrSplit && manager.joinAndSplitEnabled)
        {
            command.mergeOrSplit = true;
        }

        return command;
    }

    public virtual void InterpretCommand(CerberusCommand command)
    {
        if (command.mergeOrSplit)
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
                PlayAnimation(HopToDestination(coord, AnimationUtility.basicMoveAndPushSpeed));
                Move(coord);
                onStandardMove.Invoke();
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
                    PlayAnimation(HopToDestination(coord, AnimationUtility.basicMoveAndPushSpeed));

                    pushableEntity.Move(pushCoord);
                    Move(coord);
                    onStandardMove.Invoke();
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
        var category = spriteResolver.GetCategory();
        var label = disableAndShowPentagram ? "8" : "1";
        spriteResolver.SetCategoryAndLabel(category, label);
    }
}