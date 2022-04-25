using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[GetUndoDataReturnsNull]
public class Finish : PuzzleEntity
{
    public Finish()
    {
        entityRules = "Every Cerberus must stand on a goal tile to win the level.";
        landableScore = 0;
    }

    public void LookUnavailable()
    {
        spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
    }
    
    public void LookAvailable()
    {
        spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
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

    public override StateData GetUndoData()
    {
        return null;
    }
}
