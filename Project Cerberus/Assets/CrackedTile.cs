using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "FloorTile/Cracked")]
public class CrackedTile : FloorTile
{
    private int stage = 0;
    [SerializeField] private int initialState = 0;
    [SerializeField] private Sprite[] crackStageSprite = new Sprite[4];

    public CrackedTile()
    {
        if (initialState < 3)
        {
            landable = true;
            jumpable = false;
        }
        else
        {
            landable = false;
            jumpable = true;
        }
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);
        var idx = Mathf.Clamp(initialState + stage, 0, crackStageSprite.Length - 1);
        tileData.sprite = crackStageSprite[idx];
    }
}