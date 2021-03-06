using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "FloorTile/Cracked")]
public class CrackedTile : FloorTile
{
    public class CrackedTileStateData : StateData
    {
        public CrackedTile tile;
        public byte stage => myByte;

        public CrackedTileStateData(CrackedTile tile, int stage)
        {
            this.tile = tile;
            // NOTE: In order not to ruin serialization of counter, I am not refactoring the type of "stage."
            myByte = (byte)stage;
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
    [ShowInTileInspector] public int initialState = 0;
    [SerializeField] private Sprite[] crackStageSprite = new Sprite[4];

    public CrackedTile()
    {
        needsToBeCloned = true;
        allowsAllSuperPushedEntitiesPassage = true;
    }

    public void Awake()
    {
        needsToBeCloned = true;
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
        // Check that the other puzzle entity is not a destructible block that was destroyed, nor a super pushed block.
        bool otherIsDestroyedWood = (other is WoodBlock woodBlock) && woodBlock.isDestroyed;
        if (!(other.isSuperPushed || otherIsDestroyedWood))
        {
            stage += 1;
            var popup = TextPopup.Create((3 - stage).ToString(), Color.yellow);
            popup.transform.position = position;
            popup.PlayRiseAndFadeAnimation();
        }

        if (stage < 3)
        {
            SetFieldsToPreFinalStatePreset();
        }
        else
        {
            SetFieldsToFinalStatePreset();
            // Make every entity on top this cell fall into a pit.
            // Note: Calling XxFallIntoPit removes entity from currentCell.puzzleEntities. This is why it's necessary to
            // make a copy puzzleEntities to iterate over.
            var copyOfPuzzleEntities = new PuzzleEntity[currentCell.puzzleEntities.Count];
            currentCell.puzzleEntities.CopyTo(copyOfPuzzleEntities);
            foreach (var entity in copyOfPuzzleEntities)
            {
                // Play falling animation.
                // Mark this entity as in a hole, for undo.
                entity.inHole = true;
                entity.PlayAnimation(entity.XxFallIntoPit(1f, 90f, 0f),
                    PuzzleEntity.PlayAnimationMode.playAfterCurrentFinished);
            }
        }
    }

    public override void OnEnterCollisionWithEntity(PuzzleEntity other)
    {
        // Check if tile is cracked through, and the other entity is not super pushed.
        if (stage == 3 && !other.isSuperPushed)
        {
            // Play falling animation.
            // Mark this entity as in a hole, for undo.
            other.inHole = true;
            other.PlayAnimation(other.XxFallIntoPit(AnimationUtility.fallDuration, AnimationUtility.fallRotationSpeed,
                AnimationUtility.fallFinalScale), PuzzleEntity.PlayAnimationMode.playAfterCurrentFinished);
        }
    }

    private void SetFieldsToPreFinalStatePreset()
    {
        landableScore = 0;
        jumpable = false;
        // stopsPlayer = false;
    }

    private void SetFieldsToFinalStatePreset()
    {
        landableScore = -100;
        jumpable = true;
        // stopsPlayer = true;
    }

    public override StateData GetUndoData()
    {
        return new CrackedTileStateData(this, stage);
    }
}