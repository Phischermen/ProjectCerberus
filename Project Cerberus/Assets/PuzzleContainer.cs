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

    private int turn = 0;
    private int _currentMove = 0;

    private List<Cerberus> _moveOrder;
    //public bool doneMakingMoves { get; protected set; }

    private Laguna _laguna;
    private Jack _jack;
    private Kahuna _kahuna;

    // Start is called before the first frame update
    void Start()
    {
        // Get components
        tilemap = GetComponentInChildren<Tilemap>();
        _jack = FindObjectOfType<Jack>();
        _kahuna = FindObjectOfType<Kahuna>();
        _laguna = FindObjectOfType<Laguna>();
        // Initialize collections
        levelMap = new LevelCell[maxLevelWidth, maxLevelHeight];
        for (int i = 0; i < maxLevelWidth; i++)
        {
            for (int j = 0; j < maxLevelHeight; j++)
            {
                levelMap[i, j] = new LevelCell();
            }
        }

        _moveOrder = new List<Cerberus>();
        if (_jack) _moveOrder.Add(_jack);
        if (_kahuna) _moveOrder.Add(_kahuna);
        if (_laguna) _moveOrder.Add(_laguna);
        // Setup tilemap for parsing
        tilemap.CompressBounds();
        var bounds = tilemap.cellBounds;
        if (tilemap.size.x > 32 || tilemap.size.y > 32)
        {
            NZ.NotifyZach(string.Format("Level is too big; level must be {0} x {1}", 32, 32));
        }

        // Add entities to map
        foreach (var entity in FindObjectsOfType<PuzzleEntity>())
        {
            AddEntityToCell(entity);
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
                    NZ.NotifyZach(string.Format("Invalid tile found at {0}. Please replace with valid Tile.",
                        new Vector2Int(i, j)));
                }

                // Set levelCell's floor tile
                var levelCell = levelMap[i, j];
                levelCell.floorTile = floorTile;
            }
        }
        // Populate levelMap
    }

    // Update is called once per frame
    void Update()
    {
        var currentCerberus = _moveOrder[_currentMove];
        // Process movement of currently controlled cerberus
        currentCerberus.ProcessMoveInput();
        if (currentCerberus.doneWithMove)
        {
            _currentMove += 1;
            if (_currentMove == _moveOrder.Count)
            {
                // All cerberus have moved. Start next turn
                IncrementTurn();
                _currentMove = 0;
            }
            // Start next cerberus's move
            var nextCerberus = _moveOrder[_currentMove];
            nextCerberus.StartMove();
        }
    }


    // Level Map management
    public void AddEntityToCell(PuzzleEntity entity)
    {
        var cell = entity.position;
        if (cell.x > 32 || cell.y > 32)
        {
            NZ.NotifyZach("Entity placed outside bounds: " + entity.name);
            return;
        }

        levelMap[cell.x, cell.y].puzzleEntities.Add(entity);
    }

    public void RemoveEntityFromCell(PuzzleEntity entity)
    {
        var cell = entity.position;
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

    

    // Move order management
    void SubmitMoves()
    {
    }

    void ChangeCerberusSpot(Cerberus cerberus, int newSpot)
    {
    }

    // Turn management
    void IncrementTurn()
    {
        turn += 1;
        _currentMove = 0;
        Debug.Log(turn);
    }

    void GoBackToTurn(int newTurn)
    {
    }
    
}