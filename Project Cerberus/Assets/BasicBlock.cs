using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicBlock : PuzzleEntity
{
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
        jumpable = true;
    }
}
