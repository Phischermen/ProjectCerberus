/*
* StickySwitch stays pressed for one move after the player exits.
*/

using UnityEngine;

public class StickySwitch : Switch
{
    public class StickySwitchStateData : SwitchStateData
    {
        public StickySwitch stickySwitch;
        public byte movesElapsedSinceExit => myByte;
        public bool entityHasExited => booleans[0];

        public StickySwitchStateData(StickySwitch stickySwitch, bool flipped, int movesElapsedSinceExit, bool entityHasExited) : base(
            stickySwitch, flipped)
        {
            this.stickySwitch = stickySwitch;
            myByte = (byte)movesElapsedSinceExit;
            booleans[0] = entityHasExited;
        }

        public override void Load()
        {
            base.Load();
            stickySwitch.movesElapsedSinceExit = movesElapsedSinceExit;
            stickySwitch.entityHasExited = entityHasExited;
        }
    }

    [ShowInTileInspector, HideInInspector] public int movesElapsedSinceExit;
    public int movesSwitchStaysStuck = 1;
    private bool entityHasExited;

    protected StickySwitch()
    {
        entityRules = "StickySwitch stays pressed for one move after the player exits.";
    }

    public override void OnEnterCollisionWithEntity(PuzzleEntity other)
    {
        base.OnEnterCollisionWithEntity(other);
        movesElapsedSinceExit = 0;
        entityHasExited = false;
    }

    public override void OnExitCollisionWithEntity(PuzzleEntity other)
    {
        entityHasExited = true;
    }

    public override void OnPlayerMadeMove()
    {
        if (isPressed && entityHasExited)
        {
            var popup = TextPopup.Create((movesSwitchStaysStuck - movesElapsedSinceExit).ToString(), Color.green);
            movesElapsedSinceExit += 1;
            popup.transform.position = transform.position;
            popup.PlayRiseAndFadeAnimation();
            if (movesElapsedSinceExit > movesSwitchStaysStuck)
            {
                isPressed = false;
                movesElapsedSinceExit = 0;
            }
        }
    }

    public override StateData GetUndoData()
    {
        var undoData = new StickySwitchStateData(this, isPressed, movesElapsedSinceExit, entityHasExited);
        return undoData;
    }
}