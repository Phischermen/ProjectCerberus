using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CushionBlock : BasicBlock
{
    CushionBlock()
    {
        entityRules = "A big soft pillow. Is landable.";
        landableScore = 1;
    }
}
