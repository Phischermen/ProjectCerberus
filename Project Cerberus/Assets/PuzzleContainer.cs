using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PuzzleContainer : MonoBehaviour
{
    struct LevelCell
    {
        public FloorTile floorTile;
    }
    private LevelCell[,] _levelMap = new LevelCell[32, 32];
    private Tilemap _tilemap;
    
    // Start is called before the first frame update
    void Start()
    {
        // 
        _tilemap = GetComponentInChildren<Tilemap>();
        // Setup tilemap for parsing
        _tilemap.CompressBounds();
        var bounds = _tilemap.cellBounds;
        if (_tilemap.size.x > 32 || _tilemap.size.y > 32)
        {
            Debug.LogError(string.Format("YOU FOOL! Level is too big; level must be {0} x {1}", 32, 32));
        }
        for (var i = bounds.x; i < bounds.xMax; i++)
        {
            for (var j = bounds.y; j < bounds.yMax; j++)
            {
                // Parse tilemap
                
                // Get floor tile
                var floorTile = _tilemap.GetTile<FloorTile>(new Vector3Int(i, j, 0));
                if (floorTile != null)
                {
                    Debug.LogError(string.Format("YOU FOOL! Invalid tile found at {0}. Please replace with valid Tile.", new Vector2Int(i, j)));
                }
                // Get entities on tile
                var levelCell = new LevelCell();
                levelCell.floorTile = floorTile;
                
                _levelMap[i, j] = levelCell;
                
            }
        }
        // Populate levelMap
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
