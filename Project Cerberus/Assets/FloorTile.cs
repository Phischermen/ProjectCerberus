using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class FloorTile : Tile
{
    public bool stopsPlayer;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = sprite;
        tileData.flags = flags;
        tileData.transform = transform;
        tileData.colliderType = colliderType;
        tileData.gameObject = gameObject;
        if (position.x < 0 || position.y < 0)
        {
            tileData.color = Color.red;
            //Debug.LogError("YOU FOOL! Don't even THINK about painting there!");
        }
        else
        {
            tileData.color = color;
        }
    }
}
