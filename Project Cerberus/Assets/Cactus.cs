﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cactus : BasicBlock
{
    protected Cactus()
    {
        pushableByStandardMove = false;
        pushableByJacksSuperPush = false;
    }
}
