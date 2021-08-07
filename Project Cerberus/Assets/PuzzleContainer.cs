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

    public int turn { get; protected set; }
    public int currentMove { get; protected set; }

    public List<Cerberus> moveOrder { get; protected set; }
    //public bool doneMakingMoves { get; protected set; }

    private Laguna _laguna;
    private Jack _jack;
    private Kahuna _kahuna;
    private CerberusMajor _cerberusMajor;

    private CerberusMajorSpawnPoint _cerberusMajorSpawnPoint;

    public bool joinAndSplitEnabled { get; protected set; }
    [HideInInspector] public bool wantsToJoin;
    [HideInInspector] public bool wantsToSplit;

    private int _cerberusYetToReachGoal;

    // Start is called before the first frame update
    void Start()
    {
        // Get components
        tilemap = GetComponentInChildren<Tilemap>();
        _jack = FindObjectOfType<Jack>();
        _kahuna = FindObjectOfType<Kahuna>();
        _laguna = FindObjectOfType<Laguna>();
        _cerberusMajor = FindObjectOfType<CerberusMajor>();
        _cerberusMajorSpawnPoint = FindObjectOfType<CerberusMajorSpawnPoint>();

        // Initialize collections
        levelMap = new LevelCell[maxLevelWidth, maxLevelHeight];
        for (int i = 0; i < maxLevelWidth; i++)
        {
            for (int j = 0; j < maxLevelHeight; j++)
            {
                levelMap[i, j] = new LevelCell();
            }
        }

        moveOrder = new List<Cerberus>();
        if (_jack) moveOrder.Add(_jack);
        if (_kahuna) moveOrder.Add(_kahuna);
        if (_laguna) moveOrder.Add(_laguna);

        // Set initial gameplay variables
        if (_cerberusMajor && _cerberusMajorSpawnPoint) joinAndSplitEnabled = true;
        _cerberusYetToReachGoal = moveOrder.Count;

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
                    NZ.NotifyZach($"Invalid tile found at ({i}, {j}). Please replace with valid Tile.");
                }

                // Set levelCell's floor tile
                var levelCell = levelMap[i, j];
                levelCell.floorTile = floorTile;
            }
        }

        // Hide Cerberus
        if (_cerberusMajor) _cerberusMajor.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        var currentCerberus = moveOrder[currentMove];
        // Process movement of currently controlled cerberus
        currentCerberus.ProcessMoveInput();
        if (currentCerberus.doneWithMove)
        {
            // Check if they finished the puzzle
            if (currentCerberus.finishedPuzzle)
            {
                // Cerberus has finished puzzle. Disable control of Cerberus
                currentCerberus.gameObject.SetActive(false);
                moveOrder.Remove(currentCerberus);
                joinAndSplitEnabled = false;
                _cerberusYetToReachGoal -= 1;
                if (_cerberusYetToReachGoal == 0)
                {
                    Debug.Log("You win!");
                    gameObject.SetActive(false);
                }
            }


            // Handle request to split/join
            if (wantsToJoin && joinAndSplitEnabled)
            {
                wantsToJoin = false;
                FormCerberusMajor();
                IncrementTurn();
            }
            else if (wantsToSplit && joinAndSplitEnabled)
            {
                wantsToSplit = false;
                SplitCerberusMajor();
                IncrementTurn();
            }
            else
            {
                currentMove += 1;
                if (currentMove >= moveOrder.Count)
                {
                    // All cerberus have moved. Start next turn
                    IncrementTurn();
                }
            }

            // Start next cerberus's move
            var nextCerberus = moveOrder[currentMove];
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
        currentMove = 0;
        Debug.Log(turn);
    }

    void GoBackToTurn(int newTurn)
    {
    }

    // Merge and split Management
    public void FormCerberusMajor()
    {
        _cerberusMajor.gameObject.SetActive(true);
        _cerberusMajor.Move(_cerberusMajorSpawnPoint.position);
        _jack.gameObject.SetActive(false);
        _kahuna.gameObject.SetActive(false);
        _laguna.gameObject.SetActive(false);

        moveOrder.Clear();
        moveOrder.Add(_cerberusMajor);
    }

    public void SplitCerberusMajor()
    {
        _cerberusMajor.gameObject.SetActive(false);
        moveOrder.Clear();
        ReenableCerberusIfYetToFinishPuzzle(_jack);
        ReenableCerberusIfYetToFinishPuzzle(_kahuna);
        ReenableCerberusIfYetToFinishPuzzle(_laguna);
    }

    private void ReenableCerberusIfYetToFinishPuzzle(Cerberus cerberus)
    {
        if (!cerberus.finishedPuzzle)
        {
            cerberus.gameObject.SetActive(true);
            moveOrder.Add(cerberus);
        }
    }
}