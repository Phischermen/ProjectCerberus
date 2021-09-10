using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapBlock : BasicBlock
{
    protected TrapBlock()
    {
        entityRules = "A trap block set to catch Cerberus. It activates when jumped over.";
        jumpable = false;
    }
}
