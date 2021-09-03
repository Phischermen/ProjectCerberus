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
    public bool isSuperPushed { get; set; }

    protected Coroutine animationRoutine;
    protected bool animationIsRunning;
    protected bool animationMustStop;
    protected IEnumerator queuedAnimation;

    private static int _bezierCurveLengthEstimationSegments = 5;
    private static float _lengthEstimationDelta = 1f / _bezierCurveLengthEstimationSegments;

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
        var destinationPosition =
            puzzle.tilemap.layoutGrid.GetCellCenterWorld(new Vector3Int(destination.x, destination.y, 0));
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

        // Goto final destination
        transform.position = destinationPosition;
        animationIsRunning = false;
        animationMustStop = false;
    }

    public IEnumerator JumpAlongPath(CerberusMajor.JumpInfo[] points, float speed)
    {
        animationIsRunning = true;
        foreach (var point in points)
        {
            // Use a bezier curve to model the jump path
            var A = transform.position; // Start point
            var B = A + Vector3.up; // Control point
            var D = puzzle.tilemap.layoutGrid.GetCellCenterWorld(new Vector3Int(point.position.x, point.position.y,
                0)); // End Point
            var C = D + Vector3.up; // Control point

            // Calculate approximate distance to travel
            var distanceToTravel = 0f;
            var distanceTraveled = 0f;
            var beginningOfSegment = A;
            var interpolation = _lengthEstimationDelta;
            for (int i = 0; i < _bezierCurveLengthEstimationSegments; i++)
            {
                var endOfSegment = DeCasteljausAlgorithm(A, B, C, D, interpolation);
                distanceToTravel += Vector3.Distance(beginningOfSegment, endOfSegment);
                beginningOfSegment = endOfSegment;
                interpolation += _lengthEstimationDelta;
            }

            while (distanceTraveled < distanceToTravel && animationMustStop == false)
            {
                // Increment distance travelled
                var delta = speed * Time.deltaTime;
                distanceTraveled += delta;
                // Set position
                interpolation = distanceTraveled / distanceToTravel;
                transform.position = DeCasteljausAlgorithm(A, B, C, D, interpolation);
                yield return new WaitForFixedUpdate();
            }

            transform.position = D;
        }

        animationMustStop = false;
        animationIsRunning = false;
    }

    //The De Casteljau's Algorithm
    Vector3 DeCasteljausAlgorithm(Vector3 A, Vector3 B, Vector3 C, Vector3 D, float t)
    {
        //Linear interpolation = lerp = (1 - t) * A + t * B
        //Could use Vector3.Lerp(A, B, t)

        //To make it faster
        float oneMinusT = 1f - t;

        //Layer 1
        Vector3 Q = oneMinusT * A + t * B;
        Vector3 R = oneMinusT * B + t * C;
        Vector3 S = oneMinusT * C + t * D;

        //Layer 2
        Vector3 P = oneMinusT * Q + t * R;
        Vector3 T = oneMinusT * R + t * S;

        //Final interpolated position
        Vector3 U = oneMinusT * P + t * T;

        return U;
    }
}