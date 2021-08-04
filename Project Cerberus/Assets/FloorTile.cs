using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class FloorTile : Tile
{
    public bool stopsPlayer;
    public bool stopsBlock;
    public bool stopsFireball;
    public bool jumpable;
    public bool landable;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = sprite;
        tileData.flags = flags;
        tileData.transform = transform;
        tileData.colliderType = colliderType;
        tileData.gameObject = gameObject;
        if (position.x < 1 || position.y < 1 || position.x > PuzzleContainer.maxLevelWidth || position.y > PuzzleContainer.maxLevelHeight)
        {
            tileData.color = Color.red;
        }
        else
        {
            tileData.color = color;
        }
    }
}
