/*
 * PuzzleTriggerBox scans for puzzle entities in its cell every move and is also a puzzle entity itself. It is less
 * expensive than TransformTriggerBox, but it is limited to only scan one cell: the cell it is on top of. When
 * PuzzleTriggerBox finds an entity its scanning for, it invokes 'OnTriggered.' 
 */
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

[GetUndoDataReturnsNull]
public class PuzzleTriggerBox : PuzzleEntity
{
    public List<PuzzleEntity> objectsToScanFor;
    public UnityEvent onTrigger;

    PuzzleTriggerBox()
    {
        collisionsEnabled = false;
        // To prevent trigger box from falling into holes lol.
        isSuperPushed = true;
    }
    public override void OnPlayerMadeMove()
    {
        if (currentCell.puzzleEntities.Any(entity => objectsToScanFor.Contains(entity)))
        {
            onTrigger.Invoke();
        }
    }

    public override UndoData GetUndoData()
    {
        return null;
    }
}