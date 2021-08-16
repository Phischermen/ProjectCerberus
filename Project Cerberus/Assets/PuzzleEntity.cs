using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class PuzzleEntity : MonoBehaviour
{
    protected PuzzleContainer puzzle;
    protected GameManager manager;
    [HideInInspector] public Vector2Int position;

    public bool collisionsEnabled { get; protected set; } = true;
    public bool isStatic { get; protected set; }
    public bool isPlayer { get; protected set; }
    public bool isBlock { get; protected set; }
    public bool stopsPlayer { get; protected set; }
    public bool stopsBlock { get; protected set; }
    public bool pushableByFireball { get; protected set; }
    public bool pushable { get; protected set; }
    public bool landable { get; protected set; }

    protected virtual void Awake()
    {
        var vec3Position = FindObjectOfType<Grid>().WorldToCell(transform.position);
        position = new Vector2Int(vec3Position.x, vec3Position.y);

        puzzle = FindObjectOfType<PuzzleContainer>();
        manager = FindObjectOfType<GameManager>();
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
        var newCell = puzzle.GetCell(cell);
        var currentCell = puzzle.GetCell(position);
        puzzle.RemoveEntityFromCell(this, position);
        foreach (var currentCellPuzzleEntity in currentCell.puzzleEntities)
        {
            if (collisionsEnabled && currentCellPuzzleEntity.collisionsEnabled)
            {
                OnExitCollisionWithEntity(currentCellPuzzleEntity);
                currentCellPuzzleEntity.OnExitCollisionWithEntity(this);
            }
        }

        position = cell;
        transform.position = puzzle.tilemap.layoutGrid.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0));
        foreach (var newCellPuzzleEntity in newCell.puzzleEntities)
        {
            if (collisionsEnabled && newCellPuzzleEntity.collisionsEnabled)
            {
                OnEnterCollisionWithEntity(newCellPuzzleEntity);
                newCellPuzzleEntity.OnEnterCollisionWithEntity(this);
            }
        }

        puzzle.AddEntityToCell(this, position);
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
        return collisionsEnabled && entity.collisionsEnabled && (
            (isPlayer && entity.stopsPlayer) ||
            (isBlock && entity.stopsBlock));
    }

    public bool CollidesWith(FloorTile floorTile)
    {
        return collisionsEnabled && (
            (isPlayer && floorTile.stopsPlayer) ||
            (isBlock && floorTile.stopsBlock));
    }

    public void SetCollisionsEnabled(bool enable)
    {
        if (enable == collisionsEnabled)return;
        collisionsEnabled = enable;
        // Invoke callbacks
        var currentCell = puzzle.GetCell(position);
        if (enable)
        {
            foreach (var newCellPuzzleEntity in currentCell.puzzleEntities)
            {
                if (collisionsEnabled && newCellPuzzleEntity.collisionsEnabled)
                {
                    OnEnterCollisionWithEntity(newCellPuzzleEntity);
                    newCellPuzzleEntity.OnEnterCollisionWithEntity(this);
                }
            }
        }
        else
        {
            foreach (var newCellPuzzleEntity in currentCell.puzzleEntities)
            {
                if (collisionsEnabled && newCellPuzzleEntity.collisionsEnabled)
                {
                    OnExitCollisionWithEntity(newCellPuzzleEntity);
                    newCellPuzzleEntity.OnExitCollisionWithEntity(this);
                }
            }
        }
    }
}