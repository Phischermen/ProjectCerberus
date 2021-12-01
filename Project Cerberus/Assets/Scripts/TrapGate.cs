using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapGate : Gate
{
    protected TrapGate()
    {
        entityRules = "Trap gates can not be jumped over";
        jumpable = false;
    }

    protected override void SetFieldsToClosedPreset()
    {
        base.SetFieldsToClosedPreset();
        jumpable = false;
    }
}
