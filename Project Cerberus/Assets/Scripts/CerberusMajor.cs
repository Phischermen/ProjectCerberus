using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class CerberusMajor : Cerberus
{
    [SerializeField] private GameObject jumpArrowSource;
    private GameObject[] _jumpArrows;
    private bool goalIsOnJumpPath;

    public struct JumpInfo
    {
        public Vector2Int position;
        public float rotation;

        public override bool Equals(object obj)
        {
            if (obj is JumpInfo info)
            {
                return position == info.position;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public JumpInfo(Vector2Int position, float rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }
    }

    private List<JumpInfo> _jumpSpaces;
    private static int _maxJumpArrows = 32;

    private static int _bezierCurveLengthEstimationSegments = 5;
    private static float _lengthEstimationDelta = 1f / _bezierCurveLengthEstimationSegments;

    public AudioSource jumpSfx;

    CerberusMajor()
    {
        isCerberusMajor = true;
    }

    private void Start()
    {
        _jumpSpaces = new List<JumpInfo>();
        _jumpArrows = new GameObject[_maxJumpArrows];
        for (int i = 0; i < _maxJumpArrows; i++)
        {
            _jumpArrows[i] = Instantiate(jumpArrowSource);
            _jumpArrows[i].gameObject.SetActive(false);
            jumpArrowSource.gameObject.SetActive(false);
        }
    }

    public override void ProcessMoveInput()
    {
        base.ProcessMoveInput();
        if (input.specialHeld)
        {
            if (input.upPressed)
            {
                AddJumpSpace(Vector2Int.up, 90);
            }

            else if (input.downPressed)
            {
                AddJumpSpace(Vector2Int.down, 270);
            }

            else if (input.rightPressed)
            {
                AddJumpSpace(Vector2Int.right, 0);
            }

            else if (input.leftPressed)
            {
                AddJumpSpace(Vector2Int.left, 180);
            }
        }
        else if (input.specialReleased)
        {
            if (_jumpSpaces.Count > 0)
            {
                puzzle.PushToUndoStack();
                // Travel across jump spaces
                foreach (var jumpInfo in _jumpSpaces)
                {
                    Move(jumpInfo.position);
                }

                JumpInfo[] jumpInfoCopy = new JumpInfo[_jumpSpaces.Count];
                _jumpSpaces.CopyTo(jumpInfoCopy);
                PlayAnimation(JumpAlongPath(jumpInfoCopy, AnimationUtility.jumpSpeed));
                _jumpSpaces.Clear();
                RenderJumpPath();
                DeclareDoneWithMove();
            }
        }
        else
        {
            if (input.upPressed)
            {
                BasicMove(Vector2Int.up);
            }

            else if (input.downPressed)
            {
                BasicMove(Vector2Int.down);
            }

            else if (input.rightPressed)
            {
                BasicMove(Vector2Int.right);
            }

            else if (input.leftPressed)
            {
                BasicMove(Vector2Int.left);
            }
        }

        if (input.undoPressed)
        {
            if (_jumpSpaces.Count > 0)
            {
                _jumpSpaces.Clear();
                RenderJumpPath();
            }
            else
            {
                manager.wantsToUndo = true;
            }
        }

        if (input.cycleCharacter)
        {
            _jumpSpaces.Clear();
            RenderJumpPath();
            manager.wantsToCycleCharacter = true;
        }
    }

    private void AddJumpSpace(Vector2Int offset, float rotation)
    {
        var lastJumpPosition = (_jumpSpaces.Count > 0) ? _jumpSpaces[_jumpSpaces.Count - 1].position : position;
        var jumpedOverSpace = lastJumpPosition + offset;
        var jumpedOverCell = puzzle.GetCell(jumpedOverSpace);
        var newJumpSpace = jumpedOverSpace + offset;
        var newJumpCell = puzzle.GetCell(newJumpSpace);
        // Check if user is "backing out"
        if (newJumpSpace == position)
        {
            _jumpSpaces.Clear();
            RenderJumpPath();
            // Cerberus cannot possibly have goal in jump path.
            goalIsOnJumpPath = false;
            return;
        }

        // Check for entity to jump over and valid place to land
        var canJump = (jumpedOverCell.GetJumpableEntity() || jumpedOverCell.floorTile.jumpable) &&
                      newJumpCell.floorTile != null && newJumpCell.floorTile.landable;

        if (canJump)
        {
            // Check for collision and if landable
            var landableEntities = newJumpCell.GetLandableEntities();
            var newJumpCellLandableScore = newJumpCell.GetLandableScore();
            var canLand = (newJumpCellLandableScore >= 0) ||
                          (newJumpCell.puzzleEntities.Count == 0 && newJumpCell.floorTile.landable);
            if (canLand)
            {
                var newJumpInfo = new JumpInfo(newJumpSpace, rotation);
                // Check if space is already in collection
                if (_jumpSpaces.Contains(newJumpInfo))
                {
                    // Erase part of jump space path
                    var idxOfSpaceToRemove = _jumpSpaces.IndexOf(newJumpInfo) + 1;
                    _jumpSpaces.RemoveRange(idxOfSpaceToRemove, _jumpSpaces.Count - idxOfSpaceToRemove);
                    RenderJumpPath();
                    // Cerberus cannot possibly have goal in jump path.
                    goalIsOnJumpPath = false;
                }
                else if(!goalIsOnJumpPath)
                {
                    // Add space to path if goal is not in path
                    _jumpSpaces.Add(newJumpInfo);
                    RenderJumpPath();
                    // Check if cell being landed on has goal
                    foreach (var entity in landableEntities)
                    {
                        if (entity is Finish)
                        {
                            goalIsOnJumpPath = true;
                        }
                    }
                }
            }
        }
    }

    private void RenderJumpPath()
    {
        for (int i = 0; i < _jumpArrows.Length; i++)
        {
            var arrow = _jumpArrows[i];
            if (i >= _jumpSpaces.Count)
            {
                arrow.SetActive(false);
            }
            else
            {
                arrow.SetActive(true);
                arrow.transform.position = puzzle.GetCellCenterWorld(_jumpSpaces[i].position);
                arrow.transform.eulerAngles = new Vector3(0, 0, _jumpSpaces[i].rotation);
            }
        }
    }

    // Animations

    public IEnumerator JumpAlongPath(CerberusMajor.JumpInfo[] points, float speed)
    {
        animationIsRunning = true;
        foreach (var point in points)
        {
            // Use a bezier curve to model the jump path
            var A = transform.position; // Start point
            var B = A + Vector3.up; // Control point
            var D = puzzle.GetCellCenterWorld(point.position); // End Point
            var C = D + Vector3.up; // Control point

            // Calculate approximate distance to travel
            var distanceToTravel = 0f;
            var distanceTraveled = 0f;
            var beginningOfSegment = A;
            var interpolation = _lengthEstimationDelta;
            for (int i = 0; i < _bezierCurveLengthEstimationSegments; i++)
            {
                var endOfSegment = AnimationUtility.DeCasteljausAlgorithm(A, B, C, D, interpolation);
                distanceToTravel += Vector3.Distance(beginningOfSegment, endOfSegment);
                beginningOfSegment = endOfSegment;
                interpolation += _lengthEstimationDelta;
            }

            PlaySfxPitchShift(jumpSfx, 0.9f, 1f);

            while (distanceTraveled < distanceToTravel && animationMustStop == false)
            {
                // Increment distance travelled
                var delta = speed * Time.deltaTime;
                distanceTraveled += delta;
                // Set position
                interpolation = distanceTraveled / distanceToTravel;
                transform.position = AnimationUtility.DeCasteljausAlgorithm(A, B, C, D, interpolation);
                yield return new WaitForFixedUpdate();
            }

            transform.position = D;
            PuzzleCameraController.AddShake(0.1f);
        }

        animationMustStop = false;
        animationIsRunning = false;
    }
}