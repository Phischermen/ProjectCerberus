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
    [ShowInTileInspector] public bool collisionsEnabled { get; protected set; } = true;
    [ShowInTileInspector] public bool isStatic { get; protected set; }
    [ShowInTileInspector] public bool isPlayer { get; protected set; }
    [ShowInTileInspector] public bool isBlock { get; protected set; }
    [ShowInTileInspector] public bool stopsPlayer { get; protected set; }
    [ShowInTileInspector] public bool stopsBlock { get; protected set; }
    [ShowInTileInspector] public bool pushableByFireball { get; protected set; }
    [ShowInTileInspector] public bool interactsWithFireball { get; protected set; }
    [ShowInTileInspector] public bool pushable { get; protected set; }
    [ShowInTileInspector] public bool landable { get; protected set; }

    protected virtual void Awake()
    {
        var vec3Position = FindObjectOfType<Grid>().WorldToCell(transform.position);
        position = new Vector2Int(vec3Position.x, vec3Position.y);

        puzzle = FindObjectOfType<PuzzleContainer>();
        manager = FindObjectOfType<GameManager>();
    }

    private void Start()
    {
        // Invoke enter collision callback with puzzle entities in initial cell
        var currentCell = puzzle.GetCell(position);
        foreach (var newCellPuzzleEntity in currentCell.puzzleEntities)
        {
            if (collisionsEnabled && newCellPuzzleEntity.collisionsEnabled && newCellPuzzleEntity != this)
            {
                OnEnterCollisionWithEntity(newCellPuzzleEntity);
                newCellPuzzleEntity.OnEnterCollisionWithEntity(this);
            }
        }
    }

    public virtual void OnEnterCollisionWithEntity(PuzzleEntity other)
    {
    }

    public virtual void OnExitCollisionWithEntity(PuzzleEntity other)
    {
    }

    public virtual void OnShotByKahuna()
    {
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

        if (collisionsEnabled)
        {
            currentCell.floorTile.OnExitCollisionWithEntity(this);
            puzzle.tilemap.RefreshTile(new Vector3Int(position.x, position.y, 0));
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

        if (collisionsEnabled)
        {
            newCell.floorTile.OnEnterCollisionWithEntity(this);
            puzzle.tilemap.RefreshTile(new Vector3Int(position.x, position.y, 0));
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
        if (enable == collisionsEnabled) return;
        collisionsEnabled = enable;
        // Invoke callbacks
        var currentCell = puzzle.GetCell(position);
        if (enable)
        {
            foreach (var entity in currentCell.puzzleEntities)
            {
                if (entity.collisionsEnabled && entity != this)
                {
                    OnEnterCollisionWithEntity(entity);
                    entity.OnEnterCollisionWithEntity(this);
                }
            }
        }
        else
        {
            foreach (var entity in currentCell.puzzleEntities)
            {
                if (entity.collisionsEnabled && entity != this)
                {
                    OnExitCollisionWithEntity(entity);
                    entity.OnExitCollisionWithEntity(this);
                }
            }
        }
    }
}