using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PuzzleContainer : MonoBehaviour
{
    class LevelCell
    {
        public FloorTile floorTile;
        public List<PuzzleEntity> puzzleEntities = new List<PuzzleEntity>();
    }

    public static int maxLevelWidth = 32;
    public static int maxLevelHeight = 32;
    private LevelCell[,] _levelMap;
    private Tilemap _tilemap;

    private int turn = 0;
    private int _currentMove = 0;
    private List<Cerberus> _moveOrder;
    public bool doneMakingMoves { get; }

    private Cerberus _cerberus;

    // Start is called before the first frame update
    void Start()
    {
        // Get components
        _tilemap = GetComponentInChildren<Tilemap>();
        _cerberus = FindObjectOfType<Cerberus>();
        // Initialize collections
        _levelMap = new LevelCell[maxLevelHeight, maxLevelHeight];
        _moveOrder = new List<Cerberus>();
        _moveOrder.Add(_cerberus);
        // Setup tilemap for parsing
        _tilemap.CompressBounds();
        var bounds = _tilemap.cellBounds;
        if (_tilemap.size.x > 32 || _tilemap.size.y > 32)
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
                var floorTile = _tilemap.GetTile<FloorTile>(new Vector3Int(i, j, 0));
                if (floorTile != null)
                {
                    NZ.NotifyZach(string.Format("Invalid tile found at {0}. Please replace with valid Tile.",
                        new Vector2Int(i, j)));
                }

                // Set levelCell's floor tile
                var levelCell = _levelMap[i, j];
                levelCell.floorTile = floorTile;
            }
        }
        // Populate levelMap
    }

    // Update is called once per frame
    void Update()
    {
        if (_currentMove == _moveOrder.Count)
        {
            if (doneMakingMoves)
            {
                IncrementTurn();
            }
        }
        else
        {
            var currentCerberus = _moveOrder[_currentMove];
            // Process movement of currently controlled cerberus
            currentCerberus.ProcessMoveInput();
            if (currentCerberus.doneWithMove)
            {
                _currentMove = (_currentMove + 1) % (_moveOrder.Count + 1);
            }
        }
    }


    // Level Map management
    void AddEntityToCell(PuzzleEntity entity)
    {
        var cell = _tilemap.layoutGrid.WorldToCell(entity.transform.position);
        if (cell.x > 32 || cell.y > 32)
        {
            NZ.NotifyZach("Entity placed outside bounds: " + entity.name);
            return;
        }

        _levelMap[cell.x, cell.y].puzzleEntities.Add(entity);
    }

    void RemoveEntityFromCell(PuzzleEntity entity)
    {
        var cell = _tilemap.layoutGrid.WorldToCell(entity.transform.position);
        if (cell.x > 32 || cell.y > 32)
        {
            NZ.NotifyZach("Entity placed outside bounds: " + entity.name);
            return;
        }

        _levelMap[cell.x, cell.y].puzzleEntities.Remove(entity);
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
    }

    void GoBackToTurn(int newTurn)
    {
    }

    public void DeclareDoneMakingMoves()
    {
        
    }
}