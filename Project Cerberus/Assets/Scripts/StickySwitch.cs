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

        public StickySwitchUndoData(StickySwitch stickySwitch, bool flipped, int movesElapsedSinceExit) : base(
            stickySwitch, flipped)
        {
            this.stickySwitch = stickySwitch;
            this.movesElapsedSinceExit = movesElapsedSinceExit;
        }

        public override void Load()
        {
            base.Load();
            stickySwitch.movesElapsedSinceExit = movesElapsedSinceExit;
        }
    }

    [ShowInTileInspector, HideInInspector] public int movesElapsedSinceExit;
    public int movesSwitchStaysStuck = 1;

    protected StickySwitch()
    {
        entityRules = "StickySwitch stays pressed for one move after the player exits.";
    }

    public override void OnEnterCollisionWithEntity(PuzzleEntity other)
    {
        base.OnEnterCollisionWithEntity(other);
        movesElapsedSinceExit = 0;
    }

    public override void OnExitCollisionWithEntity(PuzzleEntity other)
    {
        // Deliberately empty
    }

    public override void OnPlayerMadeMove()
    {
        if (isPressed)
        {
            movesElapsedSinceExit += 1;
            if (movesElapsedSinceExit > movesSwitchStaysStuck)
            {
                Debug.Log("Switching off");
                isPressed = false;
                SwitchOnVisually(false);
                onReleased.Invoke();
                movesElapsedSinceExit = 0;
            }
        }
    }

    public override UndoData GetUndoData()
    {
        var undoData = new StickySwitchUndoData(this, isPressed, movesElapsedSinceExit);
        return undoData;
    }
}