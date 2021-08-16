using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PuzzleContainer : MonoBehaviour
{
    public class LevelCell
    {
        public FloorTile floorTile;
        public List<PuzzleEntity> puzzleEntities = new List<PuzzleEntity>();

        public PuzzleEntity GetPushableEntity()
        {
            foreach (var entity in puzzleEntities)
            {
                if (entity.pushable)
                    return entity;
            }

            return null;
        }

        public PuzzleEntity GetLandableEntity()
        {
            foreach (var entity in puzzleEntities)
            {
                if (entity.landable)
                    return entity;
            }

            return null;
        }

        public List<PuzzleEntity> GetStaticEntities()
        {
            var list = new List<PuzzleEntity>();
            foreach (var entity in puzzleEntities)
            {
                if (entity.isStatic)
                    list.Add(entity);
            }

            return list;
        }
    }

    public static int maxLevelWidth = 32;
    public static int maxLevelHeight = 32;
    public LevelCell[,] levelMap { get; protected set; }
    public Tilemap tilemap { get; protected set; }

    // Start is called before the first frame update
    void Awake()
    {
        // Get components
        tilemap = GetComponentInChildren<Tilemap>();


        // Initialize collections
        levelMap = new LevelCell[maxLevelWidth, maxLevelHeight];
        for (int i = 0; i < maxLevelWidth; i++)
        {
            for (int j = 0; j < maxLevelHeight; j++)
            {
                levelMap[i, j] = new LevelCell();
            }
        }


        // Setup tilemap for parsing
        tilemap.CompressBounds();
        var bounds = tilemap.cellBounds;
        if (tilemap.size.x > maxLevelWidth || tilemap.size.y > maxLevelHeight)
        {
            NZ.NotifyZach($"Level is too big; level must be {maxLevelWidth} x {maxLevelHeight}");
        }

        // Add entities to map
        foreach (var entity in FindObjectsOfType<PuzzleEntity>())
        {
            var vec3Position = tilemap.layoutGrid.WorldToCell(entity.transform.position);
            var position = new Vector2Int(vec3Position.x, vec3Position.y);
            AddEntityToCell(entity, position);
        }

        for (var i = bounds.x; i < bounds.xMax; i++)
        {
            for (var j = bounds.y; j < bounds.yMax; j++)
            {
                // Get floor tile. Check validity.
                var floorTile = tilemap.GetTile<FloorTile>(new Vector3Int(i, j, 0));
                var hasTile = tilemap.HasTile(new Vector3Int(i, j, 0));
                if (hasTile && floorTile == null)
                {
                    NZ.NotifyZach($"Invalid tile found at ({i}, {j}). Please replace with valid Tile.");
                }

                // Set levelCell's floor tile
                var levelCell = levelMap[i, j];
                levelCell.floorTile = floorTile;
            }
        }
    }


    // Level Map management
    public void AddEntityToCell(PuzzleEntity entity, Vector2Int cell)
    {
        if (cell.x > 32 || cell.y > 32)
        {
            NZ.NotifyZach("Entity placed outside bounds: " + entity.name);
            return;
        }

        levelMap[cell.x, cell.y].puzzleEntities.Add(entity);
    }

    public void RemoveEntityFromCell(PuzzleEntity entity, Vector2Int cell)
    {
        if (cell.x > 32 || cell.y > 32)
        {
            NZ.NotifyZach("Entity placed outside bounds: " + entity.name);
            return;
        }

        levelMap[cell.x, cell.y].puzzleEntities.Remove(entity);
    }

    public LevelCell GetCell(Vector2Int coord)
    {
        return levelMap[coord.x, coord.y];
    }
}