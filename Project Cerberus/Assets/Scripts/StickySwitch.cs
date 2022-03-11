/*
* StickySwitch stays pressed for one move after the player exits.
*/

using UnityEngine;

public class StickySwitch : Switch
{
    public class StickySwitchUndoData : SwitchUndoData
    {
        public StickySwitch stickySwitch;
        public int movesElapsedSinceExit;
        public bool entityHasExited;

        public StickySwitchUndoData(StickySwitch stickySwitch, bool flipped, int movesElapsedSinceExit, bool entityHasExited) : base(
            stickySwitch, flipped)
        {
            this.stickySwitch = stickySwitch;
            this.movesElapsedSinceExit = movesElapsedSinceExit;
            this.entityHasExited = entityHasExited;
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

    public override UndoData GetUndoData()
    {
        var undoData = new StickySwitchUndoData(this, isPressed, movesElapsedSinceExit, entityHasExited);
        return undoData;
    }
}