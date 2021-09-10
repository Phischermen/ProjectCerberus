using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotBlock : BasicBlock
{
    protected HotBlock()
    {
        entityRules = "Hot to the touch. Can only be pushed indirectly.";
        pullable = false;
        pushableByStandardMove = false;
        pushableByJacksSuperPush = false;
    }
}
