/*
 * PuzzleContainer manages a 2D array of level cells that store the entities residing in that cell. This 2D array is
 * called levelMap, and it is initialized on Awake(). PuzzleContainer relies on the existence of a tilemap component in
 * the scene to function. It expects the tilemap to be filled exclusively with tiles of type FloorTile. PuzzleContainer
 * has methods for adding and removing puzzle entities from level cells. PuzzleContainer provides debug info about
 * levelMap with an in-game tile inspector. PuzzleContainer is responsible for processing undoable objects, and
 * restoring previous states of the game. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Priority_Queue;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PuzzleContainer : MonoBehaviour
{
    public class LevelCell
    {
        /*
         * LevelCell holds a list of PuzzleEntity residing in the cell, and stores the FloorTile the entities sit on top
         * of. The entities are considered dynamic, and will move from cell to cell, but the floor tiles are intended to
         * be static. LevelCell also has helper methods to find entities with specific attributes or get other data
         * about the cell as necessary.
         */
        public Vector2Int position;
        public FloorTile floorTile;
        public List<PuzzleEntity> puzzleEntities = new List<PuzzleEntity>();
        public int spacesAwayFromChaseTarget; // Used by Hades for pathfinding.
        public int searchId;
        public static int numberOfSearches;

        public PuzzleEntity GetPullableEntity()
        {
            foreach (var entity in puzzleEntities)
            {
                if (entity.pullable)
                    return entity;
            }

            return null;
        }

        public PuzzleEntity GetEntityPushableByStandardMove()
        {
            foreach (var entity in puzzleEntities)
            {
                if (entity.pushableByStandardMove && !entity.inHole)
                    return entity;
            }

            return null;
        }

        public PuzzleEntity GetPushableEntityForMultiPush()
        {
            foreach (var entity in puzzleEntities)
            {
                if (entity.pushableByJacksMultiPush && !entity.inHole)
                    return entity;
            }

            return null;
        }

        public PuzzleEntity GetPushableEntityForSuperPush()
        {
            foreach (var entity in puzzleEntities)
            {
                if (entity.pushableByJacksSuperPush && !entity.inHole)
                    return entity;
            }

            return null;
        }

        public List<PuzzleEntity> GetLandableEntities()
        {
            var list = new List<PuzzleEntity>();
            foreach (var entity in puzzleEntities)
            {
                if (entity.landableScore >= 0)
                    list.Add(entity);
            }

            return list;
        }

        public int GetLandableScore()
        {
            if (floorTile == null)
            {
                return int.MinValue;
            }

            var score = floorTile.landableScore;
            foreach (var entity in puzzleEntities)
            {
                score += entity.landableScore;
            }

            return score;
        }

        public PuzzleEntity GetJumpableEntity()
        {
            foreach (var entity in puzzleEntities)
            {
                if (entity.jumpable)
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

        public List<PuzzleEntity> GetEntitesThatCannotBePushedByStandardMove()
        {
            var list = new List<PuzzleEntity>();
            foreach (var entity in puzzleEntities)
            {
                if (!entity.pushableByStandardMove)
                    list.Add(entity);
            }

            return list;
        }
    }

#if DEBUG

    private bool _inspectorShown;
    private int _frameInspectorShown;
    private bool _inspectorExpanded;
    private float inspectorWidth = 200;

    private void OnGUI()
    {
        // Show tile inspector if F1 pressed.
        // OnGUI triggers more than once, so a sanity check is needed to prevent the inspector from immediately closing
        if (Keyboard.current.f1Key.wasPressedThisFrame && _frameInspectorShown != Time.frameCount)
        {
            _inspectorShown = !_inspectorShown;
            _frameInspectorShown = Time.frameCount;
        }

        if (_inspectorShown)
        {
            var inspectorContentText = new List<string>();
            inspectorContentText.Add($"UndoStack size: {_undoStack.Count}");
            // Get cell that player is targeting
            var mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            var mouseCoord = new Vector2Int((int) mousePosition.x, (int) mousePosition.y);
            var inBounds = InBounds(mouseCoord);
            // Display information about that cell
            inspectorContentText.Add("Mouse over a tile to read data.");
            if (inBounds)
            {
                var cell = GetCell(mouseCoord);
                if (cell != null)
                {
                    inspectorContentText.Add($"Spaces away from chase target: {cell.spacesAwayFromChaseTarget}");
                    // Read fields and properties exposed to tile inspector.
                    // Read floor tile.
                    if (cell.floorTile != null)
                    {
                        // User has followed directions. Remove it from content list.
                        inspectorContentText.Remove("Mouse over a tile to read data.");
                        inspectorContentText.Add($"Floor Tile: {cell.floorTile.name}");
                        if (_inspectorExpanded)
                        {
                            // Add properties with ShowInTileInspector attribute to content list.
                            AddValidObjectMembersToContentList(cell.floorTile, inspectorContentText);
                        }
                    }

                    // Read PuzzleEntities
                    inspectorContentText.Add($"{cell.puzzleEntities.Count} Entity(s)");
                    if (_inspectorExpanded)
                    {
                        foreach (var entity in cell.puzzleEntities)
                        {
                            inspectorContentText.Add($"{entity.name}:");
                            AddValidObjectMembersToContentList(entity, inspectorContentText);
                        }
                    }
                }
            }


            // Construct GUI content
            string text = "";
            foreach (var s in inspectorContentText)
            {
                text += s + "\n";
            }

            GUIContent content = new GUIContent(text);
            GUIStyle style = GUI.skin.box;
            style.wordWrap = true;
            var height = style.CalcHeight(content, inspectorWidth);
            GUI.Box(new Rect(0, 0, inspectorWidth, height), content, style);
            _inspectorExpanded =
                GUI.Toggle(new Rect(0, height, inspectorWidth, 20), _inspectorExpanded, "Expand/Shrink");
        }
    }

    void AddValidObjectMembersToContentList(object myObject, List<string> inspectorContentText)
    {
        foreach (var memberInfo in myObject.GetType().GetMembers())
        {
            // Search for attribute.
            var attribute = memberInfo.GetCustomAttribute<ShowInTileInspector>();
            if (attribute != null)
            {
                // Get value of property.
                object value = null;
                if (memberInfo is FieldInfo fieldInfo)
                {
                    value = fieldInfo.GetValue(myObject);
                }
                else if (memberInfo is PropertyInfo propertyInfo)
                {
                    value = propertyInfo.GetValue(myObject);
                }

                // Add to content list.
                inspectorContentText.Add($"{memberInfo.Name}: {value}");
            }
        }
    }

#endif

    public static int maxLevelWidth = 33;
    public static int maxLevelHeight = 33;
    public LevelCell[,] levelMap { get; protected set; }
    public Tilemap tilemap { get; protected set; }

    private Stack<List<StateData>> _undoStack;
    private List<IUndoable> _undoables;

    private SimplePriorityQueue<PuzzleEntity> _entitiesToProcessWhenPlayerMakesMove;
    private GameManager _gameManager;

    void Awake()
    {
        //Verify transform is zeroed.
        if (transform.position != Vector3.zero)
        {
            NZ.NotifyZach("You got to zero the puzzle container's transform!");
            transform.position = Vector3.zero;
        }

        // Get components
        tilemap = GetComponentInChildren<Tilemap>();

        // Initialize levelMap
        levelMap = new LevelCell[maxLevelWidth, maxLevelHeight];
        for (int i = 0; i < maxLevelWidth; i++)
        {
            for (int j = 0; j < maxLevelHeight; j++)
            {
                var levelCell = new LevelCell();
                levelCell.position = new Vector2Int(i, j);
                levelMap[i, j] = levelCell;
            }
        }

        // Initialize _entitiesToProcessWhenPlayerMakesMove collection
        _entitiesToProcessWhenPlayerMakesMove = new SimplePriorityQueue<PuzzleEntity>();
        var tempUndoList = new List<(int, IUndoable)>();

        // Add Counters and GameManager to undoables.
        _gameManager = FindObjectOfType<GameManager>();
        tempUndoList.Add((1000 + _gameManager.transform.GetSiblingIndex(), _gameManager));
        foreach (var counter in FindObjectsOfType<Counter>())
        {
            tempUndoList.Add((2000 + counter.transform.GetSiblingIndex(), counter));
        }

        // Setup tilemap for parsing by compressing bounds.
        tilemap.CompressBounds();
        // Verify size is right.
        var bounds = tilemap.cellBounds;
        if (tilemap.size.x >= maxLevelWidth || tilemap.size.y >= maxLevelHeight)
        {
            NZ.NotifyZach($"Level is too big; level must be {maxLevelWidth} x {maxLevelHeight}");
        }

        // Add entities to levelMap
        foreach (var entity in FindObjectsOfType<PuzzleEntity>())
        {
            var vec3Position = tilemap.layoutGrid.WorldToCell(entity.transform.position);
            var position = new Vector2Int(vec3Position.x, vec3Position.y);
            AddEntityToCell(entity, position);
            // Add entity to undoables if necessary
            if (entity.GetType().GetCustomAttribute<GetUndoDataReturnsNull>() == null)
            {
                tempUndoList.Add((3000 + entity.transform.GetSiblingIndex(), entity));
            }

            // Add entity to _entitiesToProcessWhenPlayerMakesMove if virtual method OnPlayerMadeMove is overridden
            var methodInfo = entity.GetType().GetMethod(nameof(PuzzleEntity.OnPlayerMadeMove));
            if (methodInfo.DeclaringType != typeof(PuzzleEntity))
            {
                _entitiesToProcessWhenPlayerMakesMove.Enqueue(entity, entity.processPriority);
            }
        }

        // Parse tilemap for FloorTiles
        for (var i = bounds.x; i < bounds.xMax; i++)
        {
            for (var j = bounds.y; j < bounds.yMax; j++)
            {
                // Get FloorTile. Check validity.
                var floorTile = tilemap.GetTile<FloorTile>(new Vector3Int(i, j, 0));
                var hasTile = tilemap.HasTile(new Vector3Int(i, j, 0));
                if (hasTile && floorTile == null)
                {
                    NZ.NotifyZach($"Invalid tile found at ({i}, {j}). Please replace with valid Tile.");
                }
                else if (floorTile != null)
                {
                    var levelCell = levelMap[i, j];

                    if (!floorTile.needsToBeCloned)
                    {
                        // Initialize floorTile
                        floorTile.puzzle = this;
                        // Set floor tile in levelCell
                        levelCell.floorTile = floorTile;
                    }
                    else
                    {
                        // Set levelCell's floor tile to a clone so floor tile may have unique data
                        // Note: Cloned tiles will have '(Clone)' appended to their name.
                        var floorTileClone = Instantiate(floorTile);
                        // Initialize floorTileClone
                        floorTileClone.puzzle = this;
                        floorTileClone.currentCell = levelCell;
                        floorTileClone.position = tilemap.GetCellCenterWorld(new Vector3Int(i, j, 0));
                        levelCell.floorTile = floorTileClone;
                        tilemap.SetTile(new Vector3Int(i, j, 0), floorTileClone);
                        // Add floorTileClone to undoables. We only do this with clones because data in non-clones is
                        // expected to stay constant.
                        if (floorTileClone.GetType().GetCustomAttribute<GetUndoDataReturnsNull>() == null)
                        {
                            tempUndoList.Add((4000 + (bounds.xMax * i) + j, floorTileClone));
                        }
                    }
                }
            }
        }

        // Initialize Undo collections.
        _undoables = new List<IUndoable>();
        tempUndoList.OrderBy(tuple => tuple.Item1).ToList().ForEach(tuple => _undoables.Add(tuple.Item2));
        _undoStack = new Stack<List<StateData>>();
    }

    public void ProcessEntitiesInResponseToPlayerMove()
    {
        foreach (var entity in _entitiesToProcessWhenPlayerMakesMove)
        {
            entity.OnPlayerMadeMove();
        }
    }

    // Level Map management
    public void AddEntityToCell(PuzzleEntity entity, Vector2Int cell)
    {
        if (!InBounds(cell))
        {
            NZ.NotifyZach("Entity placed outside bounds: " + entity.name);
            return;
        }

        levelMap[cell.x, cell.y].puzzleEntities.Add(entity);
    }

    public void RemoveEntityFromCell(PuzzleEntity entity, Vector2Int cell)
    {
        if (!InBounds(cell))
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

    public Vector3 GetCellCenterWorld(Vector2Int coord)
    {
        return tilemap.layoutGrid.GetCellCenterWorld(new Vector3Int(coord.x, coord.y, 0));
    }

    public bool InBounds(Vector2Int coord)
    {
        return coord.x >= 0 && coord.x < maxLevelWidth && coord.y >= 0 && coord.y < maxLevelHeight;
    }

    // Undo system
    public void PushToUndoStack()
    {
        if (_gameManager.gameplayEnabled)
        {
            // Get undo data from every undoable, so board state can be recreated.
            var undoList = new List<StateData>();
            foreach (var undoable in _undoables)
            {
                var data = undoable.GetUndoData();
                undoList.Add(data);
            }

            _undoStack.Push(undoList);
        }
    }

    public void UndoLastMove()
    {
        // Pop from undoStack. Load the undo data.
        if (_undoStack.Count > 0)
        {
            var undoList = _undoStack.Pop();
            _gameManager.SendRPCSyncBoard(undoList.ToArray());
            foreach (var undoData in undoList)
            {
                undoData.Load();
            }

            // Refresh tiles.
            tilemap.RefreshAllTiles();
        }
    }

    public void UndoToFirstMove()
    {
        if (_undoStack.Count > 0)
        {
            // Pop from undoStack until data of the first move is uncovered.
            while (_undoStack.Count != 1)
            {
                _undoStack.Pop();
            }

            var undoList = _undoStack.Pop();
            _gameManager.SendRPCSyncBoard(undoList.ToArray());
            foreach (var undoData in undoList)
            {
                undoData.Load();
            }

            // Refresh tiles.
            tilemap.RefreshAllTiles();
        }
    }

    public StateData[] GetStateDataFromUndoables()
    {
        var data = new StateData[_undoables.Count];
        var index = 0;
        foreach (var undoable in _undoables)
        {
            data[index] = undoable.GetUndoData();
            index += 1;
        }

        return data;
    }

    public void SyncBoardWithData(StateData[] stateDatas)
    {
        var index = 0;
        foreach (var undoable in _undoables)
        {
            // NOTE: _undoables is sorted the same way across clients, enabling this loop to function.
            var myStateData = undoable.GetUndoData();
            // Copy data sent from client. Then load.
            myStateData.CopyData(stateDatas[index]);
            myStateData.Load();
            index += 1;
        }

        // Refresh tiles.
        tilemap.RefreshAllTiles();
    }
}