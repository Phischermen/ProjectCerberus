using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

public class Kahuna : Cerberus
{
    [SerializeField] private GameObject fireArrow;
    [SerializeField] private GameObject fireBall;
    [SerializeField] private GameObject explosion;
    private ParticleSystem _fireBallParticleSystem;
    private ParticleSystem _explosionParticleSystem;
    [SerializeField] private ParticleSystem fireAuraParticleSystem;
    [SerializeField] private AudioSource fireballSFX;

    // TODO sync this across clients from master client.
    Vector2Int aim = Vector2Int.zero;
    private static int _fireballRange = 32;
    private bool _specialActive;

    Kahuna()
    {
        entityRules = "Kahuna can fire fireballs, that push or interact with objects.";
    }

    protected override void Awake()
    {
        base.Awake();
        fireArrow.SetActive(false);
        // Instantiate fireball prefab for VFX. Setup emitter.
        fireBall = Instantiate(fireBall);
        _fireBallParticleSystem = fireBall.GetComponentInChildren<ParticleSystem>();
        _fireBallParticleSystem.Stop(true);
        // Instantiate fireball prefab for VFX. Setup emitter.
        explosion = Instantiate(explosion);
        _explosionParticleSystem = explosion.GetComponentInChildren<ParticleSystem>();
        _explosionParticleSystem.Stop(true);
    }

    protected new void Update()
    {
        base.Update();
        if (_specialActive != fireAuraParticleSystem.isPlaying)
        {
            if (_specialActive)
            {
                fireAuraParticleSystem.Play();
            }
            else
            {
                fireAuraParticleSystem.Stop();
                //fireAuraParticleSystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }

    public override void CheckInputForResetUndoOrCycle()
    {
        base.CheckInputForResetUndoOrCycle();
        if (input.cycleCharacter)
        {
            _specialActive = false;
            fireArrow.SetActive(false);
            manager.wantsToCycleCharacter = true;
        }

        if (input.undoPressed)
        {
            if (aim != Vector2Int.zero)
            {
                aim = Vector2Int.zero;
            }
            else
            {
                manager.wantsToUndo = true;
            }
        }
    }

    public override CerberusCommand ProcessInputIntoCommand()
    {
        var command = base.ProcessInputIntoCommand();
        command.cerberusId = 1;
        if (input.specialPressed)
        {
            command.specialActivated = true;
        }

        if (input.undoPressed)
        {
            command.specialDeactivated = true;
        }

        if ((input.specialReleased && _specialActive) || input.rightClicked)
        {
            command.specialPerformed = true;
            if (input.specialReleased)
            {
                // Specify which direction shot should go based on the client's preview arrow.
                if (aim == Vector2Int.up)
                {
                    command.specialUp = true;
                }

                else if (aim == Vector2Int.down)
                {
                    command.specialDown = true;
                }

                else if (aim == Vector2Int.right)
                {
                    command.specialRight = true;
                }

                else if (aim == Vector2Int.left)
                {
                    command.specialLeft = true;
                }
            }
        }

        if (_specialActive || input.rightClicked)
        {
            if (input.upPressed || (input.clickedCell.x == position.x && input.clickedCell.y > position.y))
            {
                command.specialUp = true;
            }

            else if (input.downPressed || (input.clickedCell.x == position.x && input.clickedCell.y < position.y))
            {
                command.specialDown = true;
            }

            else if (input.rightPressed || (input.clickedCell.y == position.y && input.clickedCell.x > position.x))
            {
                command.specialRight = true;
            }

            else if (input.leftPressed || (input.clickedCell.y == position.y && input.clickedCell.x < position.x))
            {
                command.specialLeft = true;
            }
        }
        else
        {
            if (input.upPressed || (input.clickedCell.x == position.x && input.clickedCell.y > position.y &&
                                    input.leftClicked))
            {
                command.moveUp = true;
            }

            else if (input.downPressed || (input.clickedCell.x == position.x && input.clickedCell.y < position.y &&
                                           input.leftClicked))
            {
                command.moveDown = true;
            }

            else if (input.rightPressed || (input.clickedCell.y == position.y && input.clickedCell.x > position.x &&
                                            input.leftClicked))
            {
                command.moveRight = true;
            }

            else if (input.leftPressed || (input.clickedCell.y == position.y && input.clickedCell.x < position.x &&
                                           input.leftClicked))
            {
                command.moveLeft = true;
            }
        }

        return command;
    }

    public override void InterpretCommand(CerberusCommand command)
    {
        base.InterpretCommand(command);
        fireArrow.SetActive(false);
        //var wantsToFire = false;
        if (command.specialActivated)
        {
            aim = Vector2Int.zero;
            _specialActive = true;
        }

        if (command.specialDeactivated)
        {
            _specialActive = false;
        }

        if (_specialActive || command.specialPerformed)
        {
            if (command.specialUp)
            {
                fireArrow.transform.eulerAngles = new Vector3(0, 0, 90);
                aim = Vector2Int.up;
            }

            else if (command.specialDown)
            {
                fireArrow.transform.eulerAngles = new Vector3(0, 0, 270);
                aim = Vector2Int.down;
            }

            else if (command.specialRight)
            {
                fireArrow.transform.eulerAngles = new Vector3(0, 0, 0);
                aim = Vector2Int.right;
            }

            else if (command.specialLeft)
            {
                fireArrow.transform.eulerAngles = new Vector3(0, 0, 180);
                aim = Vector2Int.left;
            }

            fireArrow.SetActive(aim != Vector2Int.zero);
        }
        else
        {
            if (command.moveUp)
            {
                BasicMove(Vector2Int.up);
            }

            else if (command.moveDown)
            {
                BasicMove(Vector2Int.down);
            }

            else if (command.moveRight)
            {
                BasicMove(Vector2Int.right);
            }

            else if (command.moveLeft)
            {
                BasicMove(Vector2Int.left);
            }
        }

        if (command.specialPerformed)
        {
            // TODO read command.specialUp/down/whatever to fire the fireball.
            _specialActive = false;
            if (aim != Vector2Int.zero)
            {
                FireBall(aim);
                fireArrow.SetActive(false);
            }
        }
    }

    private void FireBall(Vector2Int offset)
    {
        // Search for pushable block
        var searchCoord = position + offset;
        var searchCell = puzzle.GetCell(searchCoord);
        PuzzleEntity entityToPushOrInteractWith = null;
        var range = _fireballRange;
        while (range > 0)
        {
            if (searchCell.floorTile.stopsFireball)
            {
                goto AfterWhile;
            }

            foreach (var entity in searchCell.puzzleEntities)
            {
                if (entity.pushableByFireball || entity.interactsWithFireball)
                {
                    entityToPushOrInteractWith = entity;
                    goto AfterWhile;
                }
            }

            searchCoord += offset;
            searchCell = puzzle.GetCell(searchCoord);
            range -= 1;
        }

        AfterWhile:
        if (entityToPushOrInteractWith != null)
        {
            // Push or interact with entity
            if (entityToPushOrInteractWith.interactsWithFireball)
            {
                puzzle.PushToUndoStack();
                PlaySfxPitchShift(fireballSFX, 0.9f, 1.1f);
                // Get distance time to reach target
                var destinationPosition = puzzle.GetCellCenterWorld(searchCoord - offset);
                GetDistanceAndTimeToHitTarget(AnimationUtility.initialFireballSpeed,
                    AnimationUtility.fireBallAcceleration, destinationPosition, out float distanceToTravel,
                    out float time);
                // Play animations
                entityToPushOrInteractWith.PlayAnimation(entityToPushOrInteractWith.InteractWithFireball(time));
                PlayAnimation(LaunchFireball(destinationPosition, AnimationUtility.initialFireballSpeed,
                    AnimationUtility.fireBallAcceleration, distanceToTravel));
                // Interact with entity
                entityToPushOrInteractWith.OnShotByKahuna();

                hasPerformedSpecial = true;
                DeclareDoneWithMove();
            }
            else
            {
                // Push entity in front of Kahuna one space
                var pushCoord = entityToPushOrInteractWith.position + offset;
                var pushEntityNewCell = puzzle.GetCell(pushCoord);
                var pushBlocked = entityToPushOrInteractWith.CollidesWith(pushEntityNewCell.floorTile) ||
                                  entityToPushOrInteractWith.CollidesWithAny(pushEntityNewCell.puzzleEntities);
                if (!pushBlocked)
                {
                    puzzle.PushToUndoStack();
                    PlaySfxPitchShift(fireballSFX, 0.9f, 1.1f);
                    // TODO delay this sound so it plays when fireball hits target.
                    entityToPushOrInteractWith.PlaySfx(entityToPushOrInteractWith.pushedByFireballSfx);
                    // Get distance time to reach target
                    var destinationPosition = puzzle.GetCellCenterWorld(searchCoord - offset);
                    GetDistanceAndTimeToHitTarget(AnimationUtility.initialFireballSpeed,
                        AnimationUtility.fireBallAcceleration, destinationPosition, out float distanceToTravel,
                        out float time);
                    // Play animations
                    entityToPushOrInteractWith.PlayAnimation(
                        entityToPushOrInteractWith.SlideToDestination(pushCoord,
                            AnimationUtility.basicMoveAndPushSpeed, time));
                    PlayAnimation(LaunchFireball(destinationPosition, AnimationUtility.initialFireballSpeed,
                        AnimationUtility.fireBallAcceleration, distanceToTravel));
                    entityToPushOrInteractWith.Move(pushCoord);

                    hasPerformedSpecial = true;
                    DeclareDoneWithMove();
                }
                else
                {
                    PlaySfxIfNotPlaying(pushFailSFX);
                }
            }

            entityToPushOrInteractWith.onHitByFireball.Invoke();
        }
        else
        {
            PlaySfxIfNotPlaying(pushFailSFX);
        }
    }

    // Animation helper
    void GetDistanceAndTimeToHitTarget(float initialSpeed, float acceleration, Vector3 destinationPosition,
        out float distance, out float time)
    {
        distance = Vector3.Distance(transform.position, destinationPosition);
        // 𝅘𝅥𝅯 Negative b plus or minus radical! b squared minus 4ac. All over 2a! 𝅘𝅥𝅯
        time = (-initialSpeed + Mathf.Sqrt((initialSpeed * initialSpeed) + (2f * acceleration * distance))) /
               (acceleration);
    }

    // Animation

    public IEnumerator LaunchFireball(Vector3 destinationPosition, float initialSpeed, float acceleration,
        float distanceToTravel)
    {
        var startingPosition = transform.position;
        var distanceTraveled = 0f;
        var speed = initialSpeed;
        fireBall.transform.position = startingPosition;
        _fireBallParticleSystem.Play(true);
        _fireBallParticleSystem.transform.rotation = fireArrow.transform.rotation;
        while (distanceTraveled < distanceToTravel && animationMustStop == false)
        {
            // Increment speed
            speed += acceleration * Time.deltaTime;
            // Increment distance travelled
            var delta = speed * Time.deltaTime;
            distanceTraveled += delta;
            // Set position
            var interpolation = distanceTraveled / distanceToTravel;
            fireBall.transform.position = Vector3.Lerp(startingPosition, destinationPosition, interpolation);
            yield return new WaitForFixedUpdate();
        }

        fireBall.transform.position = destinationPosition;
        // Stop the fireball particle system emission, but only clear the fireball and not the sparks. 
        _fireBallParticleSystem.Clear(false);
        _fireBallParticleSystem.Stop(true);
        explosion.transform.position = destinationPosition;
        _explosionParticleSystem.Play(true);
        animationIsRunning = false;
        animationMustStop = false;
    }
}