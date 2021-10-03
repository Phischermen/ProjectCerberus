using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicBlock : PuzzleEntity
{
    public class BasicBlockUndoData : UndoData
    {
        public BasicBlock block;
        public Vector2Int position;

        public BasicBlockUndoData(BasicBlock block, Vector2Int position)
        {
            this.block = block;
            this.position = position;
        }

        public override void Load()
        {
            block.MoveForUndo(position);
            block.ResetTransformAndSpriteRendererForUndo();
        }
    }

    protected BasicBlock()
    {
        entityRules = "A basic pushable/pullable block. Not landable.";
        isBlock = true;
        stopsBlock = true;
        stopsPlayer = true;
        pullable = true;
        pushableByFireball = true;
        pushableByStandardMove = true;
        pushableByJacksMultiPush = true;
        pushableByJacksSuperPush = true;
        landableScore = -1;
        jumpable = true;
    }

    public override UndoData GetUndoData()
    {
        var undoData = new BasicBlockUndoData(this, position);
        return undoData;
    }
}