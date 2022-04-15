using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
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
        public int cerberusId;
        public bool[] commands = new bool[12];

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
            get => commands[10];
            set => commands[10] = value;
        }

        public bool skipCerberusJumpAnimation
        {
            get => commands[11];
            set => commands[11] = value;
        }

        //public List<CerberusMajor.JumpInfo> jumpPath;
        public static byte[] Serialize(object o)
        {
            var command = (CerberusCommand) o;
            int index = 0;
            var commandBytes = new byte[2 * command.commands.Length];
            for (var i = 0; i < command.commands.Length; i++)
            {
                var boolBytes = BitConverter.GetBytes(command.commands[i]);
                for (var i1 = 0; i1 < boolBytes.Length; i1++)
                {
                    commandBytes[index + i1] = boolBytes[i1];
                }

                index += boolBytes.Length;
            }

            var idBytes = BitConverter.GetBytes(command.cerberusId);
            return Combine(commandBytes, idBytes);
        }

        public static object Deserialize(byte[] data)
        {
            var command = new CerberusCommand();
            int index = 0;
            for (var i = 0; i < command.commands.Length; i++)
            {
                command.commands[i] = BitConverter.ToBoolean(data, i);
            }

            index += command.commands.Length;
            command.cerberusId = BitConverter.ToInt32(data, index);
            return command;
        }

        private static byte[] Combine(params byte[][] arrays)
        {
            // TODO make this an extension method.
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }

            return rv;
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

    class CerberusUndoData : UndoData
    {
        public Cerberus cerberus;
        public Vector2Int position;
        public bool inHole;
        public bool onTopOfGoal;
        public bool collisionDisabledAndPentagramDisplayed;
        public bool hasPerformedSpecial;

        public CerberusUndoData(Cerberus cerberus, Vector2Int position, bool inHole,
            bool collisionDisabledAndPentagramDisplayed,
            bool onTopOfGoal, bool hasPerformedSpecial)
        {
            this.cerberus = cerberus;
            this.position = position;
            this.onTopOfGoal = onTopOfGoal;
            this.hasPerformedSpecial = hasPerformedSpecial;
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
    }

    [HideInInspector] public bool doneWithMove;
    [HideInInspector, ShowInTileInspector] public bool onTopOfGoal;
    [ShowInTileInspector] public bool hasPerformedSpecial;
    [ShowInTileInspector] public bool isCerberusMajor;

    protected PuzzleGameplayInput input;

    private Sprite _cerberusSprite;
    public Sprite pentagramMarker;
    public AudioSource walkSFX;
    public AudioSource pushFailSFX;
    public AnimationCurve talkAnimationCurve;

    public UnityEvent onStandardMove;

    protected override void Awake()
    {
        base.Awake();
        input = FindObjectOfType<PuzzleGameplayInput>();
        _cerberusSprite = GetComponent<SpriteRenderer>().sprite;
    }

    public override UndoData GetUndoData()
    {
        var undoData = new CerberusUndoData(this, position, inHole,
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
                PlayAnimation(SlideToDestination(coord, AnimationUtility.basicMoveAndPushSpeed));
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
                    PlayAnimation(SlideToDestination(coord, AnimationUtility.basicMoveAndPushSpeed));

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
        landableScore = disableAndShowPentagram ? 0 : -1;
        GetComponent<SpriteRenderer>().sprite = disableAndShowPentagram ? pentagramMarker : _cerberusSprite;
    }
}