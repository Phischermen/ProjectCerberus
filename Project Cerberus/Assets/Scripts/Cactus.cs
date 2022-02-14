using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cactus : WoodBlock
{
    protected Cactus()
    {
        entityRules = "A thorny but fragile obstacle. Cannot be pushed directly";
        pushableByStandardMove = false;
        pushableByJacksSuperPush = false;
    }
    
    public override void SetFieldsToShotPreset(bool shot)
    {
        if (shot)
        {
            stopsPlayer = false;
            stopsBlock = false;
            isBlock = false;
            landableScore = 1;
            jumpable = false;
            interactsWithFireball = false;
            pullable = false;
            pushableByStandardMove = false;
            pushableByJacksMultiPush = false;
            pushableByJacksSuperPush = false;
        }
        else
        {
            stopsPlayer = true;
            stopsBlock = true;
            isBlock = true;
            landableScore = -1;
            jumpable = true;
            interactsWithFireball = true;
            // Cactuses must remain unpushable.
            pullable = true;
            pushableByStandardMove = false;
            pushableByJacksMultiPush = true;
            pushableByJacksSuperPush = false;
        }
    }
}
