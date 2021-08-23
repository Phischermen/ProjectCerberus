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
            block.position = position;
        }
    }

    protected BasicBlock()
    {
        isBlock = true;
        stopsBlock = true;
        stopsPlayer = true;
        pushableByFireball = true;
        pushable = true;
    }

    public override UndoData GetUndoData()
    {
        var undoData = new BasicBlockUndoData(this, position);
        return undoData;
    }
}