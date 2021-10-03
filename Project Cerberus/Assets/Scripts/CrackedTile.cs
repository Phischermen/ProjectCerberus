using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "FloorTile/Cracked")]
public class CrackedTile : FloorTile
{
    public class CrackedTileUndoData : UndoData
    {
        public CrackedTile tile;
        public int stage;

        public CrackedTileUndoData(CrackedTile tile, int stage)
        {
            this.tile = tile;
            this.stage = stage;
        }

        public override void Load()
        {
            tile.stage = stage;
            if (stage < 3)
            {
                tile.SetFieldsToPreFinalStatePreset();
            }
            else
            {
                tile.SetFieldsToFinalStatePreset();
            }
        }
    }

    [HideInInspector, ShowInTileInspector] public int stage = 0;
    [HideInInspector, ShowInTileInspector] public int initialState = 0;
    [SerializeField] private Sprite[] crackStageSprite = new Sprite[4];

    public CrackedTile()
    {
        needsToBeCloned = true;
        allowsAllSuperPushedEntitiesPassage = true;
    }

    public void Awake()
    {
        stage = Mathf.Clamp(initialState, 0, crackStageSprite.Length - 1);
        if (initialState < 3)
        {
            SetFieldsToPreFinalStatePreset();
        }
        else
        {
            SetFieldsToFinalStatePreset();
        }
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);
        var idx = Mathf.Clamp(stage, 0, crackStageSprite.Length - 1);
        tileData.sprite = crackStageSprite[idx];
    }

    public override void OnExitCollisionWithEntity(PuzzleEntity other)
    {
        stage += 1;
        if (stage < 3)
        {
            SetFieldsToPreFinalStatePreset();
        }
        else
        {
            SetFieldsToFinalStatePreset();
            foreach (var entity in currentCell.puzzleEntities)
            {
                // Play falling animation.
                entity.PlayAnimation(entity.FallIntoPit(1f, 90f, 0f));
            }
        }
    }

    public override void OnEnterCollisionWithEntity(PuzzleEntity other)
    {
        // Check if tile is cracked through, and the other entity is not super pushed.
        if (stage == 3 && !other.isSuperPushed)
        {
            // Play falling animation.
            other.PlayAnimation(other.FallIntoPit(1f, 90f, 0f));
        }
    }

    private void SetFieldsToPreFinalStatePreset()
    {
        landable = true;
        jumpable = false;
        // stopsPlayer = false;
    }

    private void SetFieldsToFinalStatePreset()
    {
        landable = false;
        jumpable = true;
        // stopsPlayer = true;
    }

    public override UndoData GetUndoData()
    {
        return new CrackedTileUndoData(this, stage);
    }
}