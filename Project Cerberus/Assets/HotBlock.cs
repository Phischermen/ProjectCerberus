using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotBlock : BasicBlock
{
    protected HotBlock()
    {
        pullable = false;
        pushableByStandardMove = false;
        pushableByJacksSuperPush = false;
    }
}
