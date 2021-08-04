using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PuzzleEntity : MonoBehaviour
{
    protected PuzzleContainer _puzzle;
    public Vector2Int position;

    public bool isStatic { get; protected set; }
    public bool isPlayer { get; protected set; }
    public bool isBlock { get; protected set; }
    public bool stopsPlayer { get; protected set; }
    public bool stopsBlock { get; protected set; }
    public bool stopsFireball { get; protected set; }
    public bool pushable { get; protected set; }
    public bool landable { get; protected set; }

    private void Awake()
    {
        var vec3Position = FindObjectOfType<Grid>().WorldToCell(transform.position);
        position = new Vector2Int(vec3Position.x, vec3Position.y);

        _puzzle = FindObjectOfType<PuzzleContainer>();
    }

    public virtual void OnEnterCollisionWithEntity(PuzzleEntity other)
    {
        return;
    }

    public virtual void OnExitCollisionWithEntity(PuzzleEntity other)
    {
        return;
    }

    public void Move(Vector2Int cell)
    {
        var newCell = _puzzle.GetCell(cell);
        var currentCell = _puzzle.GetCell(position);
        _puzzle.RemoveEntityFromCell(this);
        foreach (var currentCellPuzzleEntity in currentCell.puzzleEntities)
        {
            currentCellPuzzleEntity.OnExitCollisionWithEntity(this);
        }

        position = cell;
        transform.position = _puzzle.tilemap.layoutGrid.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0));
        foreach (var newCellPuzzleEntity in newCell.puzzleEntities)
        {
            newCellPuzzleEntity.OnEnterCollisionWithEntity(this);
        }

        _puzzle.AddEntityToCell(this);
    }

    public bool CollidesWithAny(List<PuzzleEntity> entities)
    {
        foreach (var entity in entities)
        {
            if (CollidesWith(entity)) return true;
        }

        return false;
    }

    public bool CollidesWith(PuzzleEntity entity)
    {
        return (isPlayer && entity.stopsPlayer) ||
               (isBlock && entity.stopsBlock);
    }

    public bool CollidesWith(FloorTile floorTile)
    {
        return (isPlayer && floorTile.stopsPlayer) ||
               (isBlock && floorTile.stopsBlock);
    }
    
    
}