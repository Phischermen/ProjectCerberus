using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PuzzleContainer : MonoBehaviour
{
    public class LevelCell
    {
        public FloorTile floorTile;
        public List<PuzzleEntity> puzzleEntities = new List<PuzzleEntity>();

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
                if (entity.pushableByStandardMove)
                    return entity;
            }

            return null;
        }

        public PuzzleEntity GetPushableEntityForMultiPush()
        {
            foreach (var entity in puzzleEntities)
            {
                if (entity.pushableByJacksMultiPush)
                    return entity;
            }

            return null;
        }

        public PuzzleEntity GetPushableEntityForSuperPush()
        {
            foreach (var entity in puzzleEntities)
            {
                if (entity.pushableByJacksSuperPush)
                    return entity;
            }

            return null;
        }

        public List<PuzzleEntity> GetLandableEntities()
        {
            var list = new List<PuzzleEntity>();
            foreach (var entity in puzzleEntities)
            {
                if (entity.landable)
                    list.Add(entity);
            }

            return list;
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
        if (Keyboard.current.f1Key.wasPressedThisFrame && _frameInspectorShown != Time.frameCount)
        {
            _inspectorShown = !_inspectorShown;
            _frameInspectorShown = Time.frameCount;
        }

        if (_inspectorShown)
        {
            // Get cell that player is targeting
            var mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            var mouseCoord = new Vector2Int((int) mousePosition.x, (int) mousePosition.y);
            var inBounds = InBounds(mouseCoord);
            // Display information about that cell
            var inspectorContentText = "Mouse over a tile to read data.";
            if (inBounds)
            {
                var cell = GetCell(mouseCoord);
                if (cell != null)
                {
                    // Read fields and properties exposed to tile inspector
                    inspectorContentText = "";
                    // Read floor tile
                    if (cell.floorTile != null)
                    {
                        inspectorContentText += $"Floor Tile: {cell.floorTile.name}\n";
                        if (_inspectorExpanded)
                        {
                            var floorTile = cell.floorTile;
                            foreach (var memberInfo in floorTile.GetType().GetMembers())
                            {
                                ShowInTileInspector attribute;
                                switch (memberInfo)
                                {
                                    case PropertyInfo propertyInfo:
                                        attribute = propertyInfo.GetCustomAttribute<ShowInTileInspector>();
                                        if (attribute != null)
                                        {
                                            inspectorContentText +=
                                                $"{propertyInfo.Name}: {propertyInfo.GetValue(floorTile)}\n";
                                        }

                                        break;
                                    case FieldInfo fieldInfo:
                                        attribute = fieldInfo.GetCustomAttribute<ShowInTileInspector>();
                                        if (attribute != null)
                                        {
                                            inspectorContentText +=
                                                $"{fieldInfo.Name}: {fieldInfo.GetValue(floorTile)}\n";
                                        }

                                        break;
                                }
                            }
                        }
                    }

                    // Read entity
                    inspectorContentText += $"{cell.puzzleEntities.Count} Entity(s)\n";
                    if (_inspectorExpanded)
                    {
                        foreach (var entity in cell.puzzleEntities)
                        {
                            inspectorContentText += $"{entity.name}:\n";
                            foreach (var memberInfo in entity.GetType().GetMembers())
                            {
                                ShowInTileInspector attribute;
                                switch (memberInfo)
                                {
                                    case PropertyInfo propertyInfo:
                                        attribute = propertyInfo.GetCustomAttribute<ShowInTileInspector>();
                                        if (attribute != null)
                                        {
                                            inspectorContentText +=
                                                $"{propertyInfo.Name}: {propertyInfo.GetValue(entity)}\n";
                                        }

                                        break;
                                    case FieldInfo fieldInfo:
                                        attribute = fieldInfo.GetCustomAttribute<ShowInTileInspector>();
                                        if (attribute != null)
                                        {
                                            inspectorContentText +=
                                                $"{fieldInfo.Name}: {fieldInfo.GetValue(entity)}\n";
                                        }

                                        break;
                                }
                            }
                        }
                    }
                }
            }


            // Construct GUI content
            GUIContent content = new GUIContent(inspectorContentText);
            GUIStyle style = GUI.skin.box;
            style.wordWrap = true;
            var height = style.CalcHeight(content, inspectorWidth);
            GUI.Box(new Rect(0, 0, inspectorWidth, height), content, style);
            _inspectorExpanded =
                GUI.Toggle(new Rect(0, height, inspectorWidth, 20), _inspectorExpanded, "Expand/Shrink");
        }
    }

#endif

    public static int maxLevelWidth = 32;
    public static int maxLevelHeight = 32;
    public LevelCell[,] levelMap { get; protected set; }
    public Tilemap tilemap { get; protected set; }

    private Stack<List<UndoData>> _undoStack;
    private List<IUndoable> _undoables;

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

        _undoables = new List<IUndoable>();
        _undoStack = new Stack<List<UndoData>>();
        
        _undoables.Add(FindObjectOfType<GameManager>());

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
            if (entity.GetType().GetCustomAttribute<GetUndoDataReturnsNull>() == null)
            {
                _undoables.Add(entity);
            }
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
                else if (floorTile != null)
                {
                    if (floorTile.GetType().GetCustomAttribute<GetUndoDataReturnsNull>() == null)
                    {
                        _undoables.Add(floorTile);
                    }

                    var levelCell = levelMap[i, j];
                    if (!floorTile.needsToBeCloned)
                    {
                        // Set floor tile
                        levelCell.floorTile = floorTile;
                    }
                    else
                    {
                        // Set levelCell's floor tile to a clone so floor tile may have unique data
                        // Note: Cloned tiles will have '(Clone)' appended to their name.
                        var floorTileClone = Instantiate(floorTile);
                        levelCell.floorTile = floorTileClone;
                        tilemap.SetTile(new Vector3Int(i, j, 0), floorTileClone);
                    }
                }
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
        var undoList = new List<UndoData>();
        foreach (var undoable in _undoables)
        {
            var data = undoable.GetUndoData();
            undoList.Add(data);
        }

        _undoStack.Push(undoList);
    }

    public void UndoLastMove()
    {
        if (_undoStack.Count > 0)
        {
            var undoList = _undoStack.Pop();
            foreach (var undoData in undoList)
            {
                undoData.Load();
            }
        }
    }
}