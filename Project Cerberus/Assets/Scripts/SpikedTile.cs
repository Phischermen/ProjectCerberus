using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FloorTile/Spiked")]
public class SpikedTile : FloorTile
{
    public SpikedTile()
    {
        allowsAllSuperPushedEntitiesPassage = true;
        jumpable = true;
        landable = false;
    }

    public override void OnEnterCollisionWithEntity(PuzzleEntity other)
    {
        // Check if tile is cracked through, and the other entity is not super pushed.
        if (other.isPlayer && !other.isSuperPushed)
        {
            // Play spiked animation.
            other.PlayAnimation(other.Spiked(AnimationUtility.spikedRotationSpeed, AnimationUtility.spikedEndPosition,
                AnimationUtility.spikedControlPointHeight, AnimationUtility.spikedSpeed));
        }
    }
}