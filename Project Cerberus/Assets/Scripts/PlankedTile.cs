using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "FloorTile/Planked")]
public class PlankedTile : FloorTile
{
    public class PlankedTileStateData : StateData
    {
        public PlankedTile tile;
        public bool broken => booleans[0];

        public PlankedTileStateData(PlankedTile tile, bool broken)
        {
            this.tile = tile;
            booleans[0] = broken;
        }

        public override void Load()
        {
            tile.broken = broken;
        }
    }
    [SerializeField] private Sprite plankBrokenSprite;
    public bool broken;
    
    public PlankedTile()
    {
        needsToBeCloned = true;
        broken = false;
    }
    
    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);
        tileData.sprite = broken ? plankBrokenSprite : sprite;
    }

    public override void OnEnterCollisionWithEntity(PuzzleEntity other)
    {
        // Check if Cerberus Major entered.
        if (other is CerberusMajor)
        {
            broken = true;
            // Play falling animation.
            other.PlayAnimation(other.XxFallIntoPit(AnimationUtility.fallDuration, AnimationUtility.fallRotationSpeed,
                AnimationUtility.fallFinalScale));
        }
    }

    public override StateData GetUndoData()
    {
        return new PlankedTileStateData(this, broken);
    }
}