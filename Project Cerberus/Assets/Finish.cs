using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Finish : PuzzleEntity
{
    public override void OnEnterCollisionWithEntity(PuzzleEntity other)
    {
        if (other is Cerberus cerberus)
        {
            cerberus.onTopOfGoal = true;
        }
    }

    public override void OnExitCollisionWithEntity(PuzzleEntity other)
    {
        if (other is Cerberus cerberus)
        {
            cerberus.onTopOfGoal = false;
        }
    }
}
