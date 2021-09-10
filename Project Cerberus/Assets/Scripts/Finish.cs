using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[GetUndoDataReturnsNull]
public class Finish : PuzzleEntity
{
    public Finish()
    {
        entityRules = "Every Cerberus must stand on a goal tile to win the level.";
        landable = true;
    }

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

    public override UndoData GetUndoData()
    {
        return null;
    }
}
