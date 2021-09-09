using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public abstract class PuzzleEntity : MonoBehaviour, IUndoable
{
    protected PuzzleContainer puzzle;
    protected GameManager manager;
    [HideInInspector] public Vector2Int position;
    public PuzzleContainer.LevelCell currentCell => puzzle.GetCell(position);
    [ShowInTileInspector] public bool collisionsEnabled { get; protected set; } = true;
    [ShowInTileInspector] public bool isStatic { get; protected set; }
    [ShowInTileInspector] public bool isPlayer { get; protected set; }
    [ShowInTileInspector] public bool isBlock { get; protected set; }
    [ShowInTileInspector] public bool stopsPlayer { get; protected set; }
    [ShowInTileInspector] public bool stopsBlock { get; protected set; }
    [ShowInTileInspector] public bool pullable { get; protected set; }
    [ShowInTileInspector] public bool pushableByFireball { get; protected set; }
    [ShowInTileInspector] public bool interactsWithFireball { get; protected set; }
    [ShowInTileInspector] public bool pushableByStandardMove { get; protected set; }
    [ShowInTileInspector] public bool pushableByJacksMultiPush { get; protected set; }
    [ShowInTileInspector] public bool pushableByJacksSuperPush { get; protected set; }
    [ShowInTileInspector] public bool landable { get; protected set; }
    [ShowInTileInspector] public bool jumpable { get; protected set; }
    public string entityRules { get; protected set; } = "No rules have been written for this object.";
    public bool isSuperPushed { get; set; }

    protected Coroutine animationRoutine;
    protected bool animationIsRunning;
    protected bool animationMustStop;
    protected IEnumerator queuedAnimation;

    [HideInInspector] public AudioSource pushedSfx;

    [HideInInspector] public AudioSource superPushedSfx;

    [HideInInspector] public AudioSource pushedByFireballSfx;

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
        foreach (var newCellPuzzleEntity in currentCell.puzzleEntities)
        {
            if (collisionsEnabled && newCellPuzzleEntity.collisionsEnabled && newCellPuzzleEntity != this)
            {
                OnEnterCollisionWithEntity(newCellPuzzleEntity);
                newCellPuzzleEntity.OnEnterCollisionWithEntity(this);
            }
        }
    }

    private void Update()
    {
        if (queuedAnimation != null && !animationIsRunning)
        {
            PlayAnimation(queuedAnimation);
            queuedAnimation = null;
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

        // Note: Transform needs to be updated via animation
        position = cell;

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

    // This version of move does not trigger 'OnEnter' or 'OnExit' callbacks
    public void MoveForUndo(Vector2Int cell)
    {
        puzzle.RemoveEntityFromCell(this, position);
        position = cell;
        transform.position = puzzle.tilemap.layoutGrid.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0));
        puzzle.AddEntityToCell(this, position);
    }

    public bool CollidesWithAny(List<PuzzleEntity> entities)
    {
        foreach (var entity in entities)
        {
            // Entities cannot collide with themselves
            if (entity == this) continue;
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
        if (!collisionsEnabled)
        {
            return false;
        }

        // Jack's super push allows entities to 'sail' over certain tiles like pits and spikes.
        if (isSuperPushed && floorTile.allowsAllSuperPushedEntitiesPassage)
        {
            return false;
        }

        return (isPlayer && floorTile.stopsPlayer) ||
               (isBlock && floorTile.stopsBlock);
    }

    public bool CollidesWith(PuzzleContainer.LevelCell levelCell)
    {
        return CollidesWith(levelCell.floorTile) || CollidesWithAny(levelCell.puzzleEntities);
    }

    public void SetCollisionsEnabled(bool enable)
    {
        if (enable == collisionsEnabled) return;
        collisionsEnabled = enable;
        // Invoke callbacks
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

            currentCell.floorTile.OnEnterCollisionWithEntity(this);
            puzzle.tilemap.RefreshTile(new Vector3Int(position.x, position.y, 0));
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

            currentCell.floorTile.OnExitCollisionWithEntity(this);
            puzzle.tilemap.RefreshTile(new Vector3Int(position.x, position.y, 0));
        }
    }

    public abstract UndoData GetUndoData();

    // Sound effects
    /* KF 9/7/21 I am considering using Wwise for our project because even implementing basic pitch shifting is kind of
     a pain. I do not like how minimum and maximum pitch is hard coded, because I know I am not going to be consistent 
     throughout the entire project unless I cache those values somehow. If Wwise doesn't work out, I think I may try
     using attributes to indicate that a sound effect is meant to be pitch shifted. */

    public void PlaySfx(AudioSource source)
    {
        if (source)
        {
            source.Play();
        }
    }

    public void StopSfx(AudioSource source)
    {
        if (source)
        {
            source.Stop();
        }
    }

    public void PlaySfxPitchShift(AudioSource source, float min, float max)
    {
        if (source)
        {
            source.pitch = Random.Range(min, max);
            source.Play();
        }
    }

    // Animations
    public void PlayAnimation(IEnumerator animationToPlay)
    {
        if (animationIsRunning)
        {
            animationMustStop = true;
            // Queue animation routine for next frame
            queuedAnimation = animationToPlay;
        }
        else
        {
            // Start animation routine 
            animationRoutine = StartCoroutine(animationToPlay);
        }
    }

    public void FinishCurrentAnimation()
    {
        if (animationIsRunning)
        {
            animationMustStop = true;
        }
    }

    public IEnumerator SlideToDestination(Vector2Int destination, float speed)
    {
        animationIsRunning = true;
        var startingPosition = transform.position;
        var destinationPosition = puzzle.GetCellCenterWorld(destination);
        var distanceToTravel = Vector3.Distance(startingPosition, destinationPosition);
        var distanceTraveled = 0f;
        while (distanceTraveled < distanceToTravel && animationMustStop == false)
        {
            // Increment distance travelled
            var delta = speed * Time.deltaTime;
            distanceTraveled += delta;
            // Set position
            var interpolation = distanceTraveled / distanceToTravel;
            transform.position = Vector3.Lerp(startingPosition, destinationPosition, interpolation);
            yield return new WaitForFixedUpdate();
        }

        StopSfx(superPushedSfx);
        // Goto final destination
        transform.position = destinationPosition;
        animationIsRunning = false;
        animationMustStop = false;
    }
}