/*
 * PuzzleEntity is the dynamic component of our puzzle. They have a lot of boolean fields/properties that define the way
 * they interact with the player/other blocks. Puzzle entities are animated procedurally with coroutines, that are
 * played via PlayAnimation(). Animations cannot be cancelled, but they can fast forward to the end by calling
 * FinishCurrentAnimation(). PuzzleEntity also has virtual methods to respond to certain events, like when it is shot by
 * Kahuna or when another PuzzleEntity enters/exits their cell. In the context of this class, "collisions" refer to when
 * PuzzleEntity A cannot share the same cell as PuzzleEntity B based on the values of their respective fields/properties.
 * Moving a PuzzleEntity across the board is done by calling Move() and then playing some kind of animation to update
 * the transform. In the context of undoing a move, MoveForUndo() must be used to reset the transform.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public abstract class PuzzleEntity : MonoBehaviour, IUndoable
{
    protected PuzzleContainer puzzle;
    protected GameManager manager;
    protected SpriteRenderer spriteRenderer;

    [HideInInspector] public Vector2Int position;
    public PuzzleContainer.LevelCell currentCell => puzzle.GetCell(position);

    [ShowInTileInspector] public bool collisionsEnabled { get; protected set; } = true;
    [ShowInTileInspector] public bool isStatic { get; protected set; }
    [ShowInTileInspector] public bool isPlayer { get; protected set; }
    [ShowInTileInspector] public bool isHades { get; protected set; }
    [ShowInTileInspector] public bool isBlock { get; protected set; }
    [ShowInTileInspector] public bool stopsPlayer { get; protected set; }
    [ShowInTileInspector] public bool stopsHades { get; protected set; }
    [ShowInTileInspector] public bool stopsBlock { get; protected set; }
    [ShowInTileInspector, HideInInspector] public bool inHole;
    [ShowInTileInspector] public bool pullable { get; protected set; }
    [ShowInTileInspector] public bool pushableByFireball { get; protected set; }
    [ShowInTileInspector] public bool interactsWithFireball { get; protected set; }
    [ShowInTileInspector] public bool pushableByStandardMove { get; protected set; }
    [ShowInTileInspector] public bool pushableByJacksMultiPush { get; protected set; }
    [ShowInTileInspector] public bool pushableByJacksSuperPush { get; protected set; }
    [ShowInTileInspector] public int landableScore { get; protected set; }
    [ShowInTileInspector] public bool jumpable { get; protected set; }
    public string entityRules { get; protected set; } = "No rules have been written for this object.";
    public bool isSuperPushed { get; set; }
    [HideInInspector] public float processPriority;

    protected Coroutine animationRoutine;
    protected bool animationIsRunning;
    protected bool animationMustStop;
    protected IEnumerator queuedAnimation;

    [HideInInspector] public bool showOptionalSfx;
    [HideInInspector] public AudioSource pushedSfx;
    [HideInInspector] public AudioSource superPushedSfx;
    [HideInInspector] public AudioSource pushedByFireballSfx;

    [HideInInspector] public bool showOptionalEvents;
    [HideInInspector] public UnityEvent onStandardPushed;
    [HideInInspector] public UnityEvent onSuperPushed;
    [HideInInspector] public UnityEvent onMultiPushed;
    [HideInInspector] public UnityEvent onHitByFireball;
    [HideInInspector] public UnityEvent onPulled;

    protected virtual void Awake()
    {
        // Initialize position.
        var vec3Position = FindObjectOfType<Grid>().WorldToCell(transform.position);
        position = new Vector2Int(vec3Position.x, vec3Position.y);
        // Get objects.
        puzzle = FindObjectOfType<PuzzleContainer>();
        manager = FindObjectOfType<GameManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // Invoke enter collision callback with puzzle entities in initial cell.
        foreach (var newCellPuzzleEntity in currentCell.puzzleEntities)
        {
            if (collisionsEnabled && newCellPuzzleEntity.collisionsEnabled && newCellPuzzleEntity != this)
            {
                OnEnterCollisionWithEntity(newCellPuzzleEntity);
                newCellPuzzleEntity.OnEnterCollisionWithEntity(this);
            }
        }

        // Invoke enter collision callback with initial floorTile
        currentCell.floorTile.OnEnterCollisionWithEntity(this);
    }

    protected void Update()
    {
        // Check for queued animation. Play it if there's not another animation already running.
        if (queuedAnimation != null && !animationIsRunning)
        {
            PlayAnimation(queuedAnimation);
            queuedAnimation = null;
        }
    }

    // Called every time player does anything to increment move counter.
    public virtual void OnPlayerMadeMove()
    {
    }

    public virtual void OnEnterCollisionWithEntity(PuzzleEntity other)
    {
    }

    public virtual void OnExitCollisionWithEntity(PuzzleEntity other)
    {
    }

    // The following two methods are related to entities that react to being hit by Kahuna's fireball.
    // This method should be used to update entity data.
    public virtual void OnShotByKahuna()
    {
    }
    
    // This method should be used to update the sprites and launch effects.
    public virtual void OnShotByKahunaVisually()
    {
    }

    public void Move(Vector2Int toCell, bool doNotTriggerOnEnter = false, bool doNotTriggerOnExit = false,
        bool doNotRefreshTiles = false)
    {
        // Get cell we're moving to.
        var newCell = puzzle.GetCell(toCell);
        // Remove ourselves from that cell.
        puzzle.RemoveEntityFromCell(this, position);
        if (!doNotTriggerOnExit && collisionsEnabled)
        {
            // Invoke the exit collision callback for every entity we are leaving behind.
            // Note: Although we technically "removed" ourselves from our current cell, currentCell still references the cell
            // we were at last.
            foreach (var currentCellPuzzleEntity in currentCell.puzzleEntities)
            {
                if (currentCellPuzzleEntity.collisionsEnabled)
                {
                    OnExitCollisionWithEntity(currentCellPuzzleEntity);
                    currentCellPuzzleEntity.OnExitCollisionWithEntity(this);
                }
            }

            // Invoke exit collision callback for the floorTile we left behind.
            currentCell.floorTile.OnExitCollisionWithEntity(this);
            if (!doNotRefreshTiles)
            {
                // Refresh the tile we left in case its state has changed from its callback.
                puzzle.tilemap.RefreshTile(new Vector3Int(position.x, position.y, 0));
            }
        }

        // Update our position.
        // Note: Transform needs to be updated via animation
        position = toCell;
        if (!doNotTriggerOnEnter && collisionsEnabled)
        {
            // Invoke enter collision callback for all the new entities we are meeting in our new cell.
            foreach (var newCellPuzzleEntity in newCell.puzzleEntities)
            {
                if (newCellPuzzleEntity.collisionsEnabled)
                {
                    OnEnterCollisionWithEntity(newCellPuzzleEntity);
                    newCellPuzzleEntity.OnEnterCollisionWithEntity(this);
                }
            }

            // Invoke enter collision callback for all the new floorTile we are meeting in our new cell.
            newCell.floorTile.OnEnterCollisionWithEntity(this);
            if (!doNotRefreshTiles)
            {
                // Refresh the tile we entered in case its state has changed from its callback.
                puzzle.tilemap.RefreshTile(new Vector3Int(position.x, position.y, 0));
            }
        }

        // Register that this entity belongs to the new cell.
        puzzle.AddEntityToCell(this, position);
    }

    // This version of move does not trigger 'OnEnter' or 'OnExit' callbacks
    public void MoveForUndo(Vector2Int cell)
    {
        // Stop animation
        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
            animationIsRunning = false;
            animationMustStop = false;
        }

        // Perform standard move
        puzzle.RemoveEntityFromCell(this, position);
        position = cell;
        transform.position = puzzle.tilemap.layoutGrid.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0));
        puzzle.AddEntityToCell(this, position);
    }

    public void ResetTransformAndSpriteRendererForUndo()
    {
        spriteRenderer.color = Color.white;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
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
            (stopsPlayer && entity.isPlayer) ||
            (isHades && entity.stopsHades) ||
            (stopsHades && entity.isHades) ||
            (isBlock && entity.stopsBlock) ||
            (stopsBlock && entity.isBlock));
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

    public void SetCollisionsEnabled(bool enable, bool invokeCallbacks = true)
    {
        // Avoid accidental invocation of callbacks when setting to same value twice.
        if (enable == collisionsEnabled) return;
        collisionsEnabled = enable;
        // Invoke callbacks.
        if (invokeCallbacks)
        {
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
                // Refresh the tile we entered.
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
                // Refresh the tile we exited.
                puzzle.tilemap.RefreshTile(new Vector3Int(position.x, position.y, 0));
            }
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
    private static Dictionary<Type, bool> animationFatalityMap = new Dictionary<Type, bool>();

    public enum PlayAnimationMode
    {
        playAfterCurrentFinished,
        finishCurrentAndPlayImmediately
    }

    public void PlayAnimation(IEnumerator animationToPlay,
        PlayAnimationMode mode = PlayAnimationMode.finishCurrentAndPlayImmediately)
    {
        // Check if the animation about to be played is fatal.
        if (animationFatalityMap.TryGetValue(animationToPlay.GetType(), out bool isFatal) == false)
        {
            // Note: Since I am working with an IEnumerator, I can not use custom attributes to mark certain animations
            // as fatal. Instead, I prefix animations that are fatal with 'Xx' so I can look it up via name.
            var fullName = animationToPlay.GetType().FullName;
            var carret1 = fullName.IndexOf('<');
            fullName = fullName.Substring(carret1, 3);
            isFatal = fullName == "<Xx";
            // Cache result
            animationFatalityMap.Add(animationToPlay.GetType(), isFatal);
        }

        // Check that fatal animation is being played with the player.
        isFatal = isFatal && isPlayer && collisionsEnabled;
        if (isFatal)
        {
            // Deter player from killing more dogs.
            manager.gameplayEnabled = false;
        }

        if (animationIsRunning)
        {
            if (mode == PlayAnimationMode.finishCurrentAndPlayImmediately)
            {
                animationMustStop = true;
            }

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

    public void FinishAllQueuedAnimations()
    {
    }
    /*
     * PuzzleEntity animations are procedural. They take the form of coroutines. An animation must follow this pattern:
     
     public IEnumerator AnimationName(Vector3 animationParameters)
     {
         // Set this to true
         animationIsRunning = true;
         // Be able to stop at any point.
         while(blah blah blah && animationMustStop == false)
         {
             
         }
         // Goto final state at end
         transform.position = animationParameters;
         // Set these to false
         animationIsRunning = false;
         animationMustStop = false;
     }
     
     If an animation will kill the player and end the game, its name must start with 'Xx'
     */

    public IEnumerator SlideToDestination(Vector2Int destination, float speed, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);
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

    public IEnumerator XxFallIntoPit(float fallDuration, float rotationSpeed, float finalScale)
    {
        animationIsRunning = true;
        // Determine if the entity that has fallen is the player in merged state.
        var playerIsFallingInPitInMergedState = isPlayer && !collisionsEnabled;
        var playerIsFallingInPitInSplitState = isPlayer && collisionsEnabled;
        if (playerIsFallingInPitInMergedState)
        {
            // Cerberus is in its merged state, and the entity currently falling into a pit is a sigill, so prevent
            // player from splitting. 
            manager.joinAndSplitEnabled = false;
        }

        // Mark this entity as in a hole, for undo.
        inHole = true;
        // Remove from puzzle container so it can't be interacted with.
        // Note: MoveForUndo() will put entity back into puzzle container.
        puzzle.RemoveEntityFromCell(this, position);
        var timeEllapsed = 0f;
        while (timeEllapsed < fallDuration && animationMustStop == false)
        {
            timeEllapsed += Time.deltaTime;
            var interpolation = timeEllapsed / fallDuration;
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * finalScale, interpolation);
            spriteRenderer.color = Color.Lerp(Color.white, Color.black, interpolation);
            yield return new WaitForFixedUpdate();
        }

        // Goto final state at end
        transform.localScale = Vector3.one * finalScale;
        spriteRenderer.color = Color.black;
        if (playerIsFallingInPitInSplitState)
        {
            // End game if this entity was in fact the player falling into a pit.
            manager.EndGameWithFailureStatus();
        }

        // Set these to false
        animationIsRunning = false;
        animationMustStop = false;
    }

    public IEnumerator XxSpiked(float rotationSpeed, Vector2 fallDelta, float controlPointHeight, float speed)
    {
        animationIsRunning = true;
        // Use a bezier curve to model the path
        var A = transform.position; // Start point
        var B = A + Vector3.up * controlPointHeight; // Control point
        var D = new Vector3(A.x + fallDelta.x, A.y + fallDelta.y, 0f); // End Point
        var C = D + Vector3.up * controlPointHeight; // Control point

        // Calculate approximate distance to travel
        var distanceTraveled = 0f;
        var interpolation = 0f;
        var distanceToTravel = AnimationUtility.ApproximateLengthOfBezierCurve(A, B, C, D);

        while (distanceTraveled < distanceToTravel && animationMustStop == false)
        {
            // Increment distance travelled
            var delta = speed * Time.deltaTime;
            distanceTraveled += delta;
            // Set position
            interpolation = distanceTraveled / distanceToTravel;
            transform.position = AnimationUtility.DeCasteljausAlgorithm(A, B, C, D, interpolation);
            // Set rotation
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
            yield return new WaitForFixedUpdate();
        }

        // Goto final state at end
        transform.position = D;

        manager.EndGameWithFailureStatus();

        // Set these to false
        animationIsRunning = false;
        animationMustStop = false;
    }
    
    public IEnumerator InteractWithFireball(float time)
    {
        animationIsRunning = true;
        var timePassed = 0f;

        while(timePassed < time && animationMustStop == false)
        {
            timePassed += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        // Make entity react to being hit by fireball.
        OnShotByKahunaVisually();
        // Set these to false
        animationIsRunning = false;
        animationMustStop = false;
    }
}