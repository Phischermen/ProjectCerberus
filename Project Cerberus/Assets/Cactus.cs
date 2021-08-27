using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cactus : BasicBlock
{
    public Cactus()
    {
        pushableByStandardMove = false;
        pushableByJacksSuperPush = false;
    }
}
