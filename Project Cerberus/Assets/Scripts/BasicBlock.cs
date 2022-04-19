using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicBlock : PuzzleEntity
{
    public class BasicBlockStateData : StateData
    {
        public BasicBlock block;
        public Vector2Int position => myVector2Int;
        public bool inHole => booleans[0];

        public BasicBlockStateData(BasicBlock block, Vector2Int position, bool inHole)
        {
            this.block = block;
            myVector2Int = position;
            booleans[0] = inHole;
        }

        public override void Load()
        {
            block.inHole = booleans[0];
            if (!inHole)
            {
                block.MoveForUndo(position);
                block.ResetTransformAndSpriteRendererForUndo();
            }
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

    public override StateData GetUndoData()
    {
        var undoData = new BasicBlockStateData(this, position, inHole);
        return undoData;
    }
}