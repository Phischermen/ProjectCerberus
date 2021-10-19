using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FloorTile/Spiked"), GetUndoDataReturnsNull]
public class SpikedTile : FloorTile
{
    public SpikedTile()
    {
        allowsAllSuperPushedEntitiesPassage = true;
        jumpable = true;
        landableScore = -1;
        needsToBeCloned = true;
    }

    public override void OnEnterCollisionWithEntity(PuzzleEntity other)
    {
        // Check if tile is cracked through, and the other entity is not super pushed.
        if (other.isPlayer && !other.isSuperPushed && currentCell.GetLandableScore() < 0)
        {
            // Play spiked animation.
            other.PlayAnimation(other.Spiked(AnimationUtility.spikedRotationSpeed, AnimationUtility.spikedEndPosition,
                AnimationUtility.spikedControlPointHeight, AnimationUtility.spikedSpeed));
        }
    }
}