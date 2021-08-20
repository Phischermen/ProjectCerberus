using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "FloorTile/Basic")]
public class FloorTile : Tile
{
    [HideInInspector] public bool needsToBeCloned;
    [ShowInTileInspector] public bool stopsPlayer;
    [ShowInTileInspector] public bool stopsBlock;
    [ShowInTileInspector] public bool stopsFireball;
    [ShowInTileInspector] public bool jumpable;
    [ShowInTileInspector] public bool landable;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = sprite;
        tileData.flags = flags;
        tileData.transform = transform;
        tileData.colliderType = colliderType;
        tileData.gameObject = gameObject;
        if (position.x < 1 || position.y < 1 || position.x > PuzzleContainer.maxLevelWidth ||
            position.y > PuzzleContainer.maxLevelHeight)
        {
            tileData.color = Color.red;
        }
        else
        {
            tileData.color = color;
        }
    }

    public virtual void OnEnterCollisionWithEntity(PuzzleEntity other)
    {
    }

    public virtual void OnExitCollisionWithEntity(PuzzleEntity other)
    {
    }
}