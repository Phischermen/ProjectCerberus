using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Finish : PuzzleEntity
{
    public override void OnEnterCollisionWithEntity(PuzzleEntity other)
    {
        if (other is Cerberus)
        {
            var cerberus = (Cerberus) other;
            cerberus.finishedPuzzle = true;
        }
    }
}
