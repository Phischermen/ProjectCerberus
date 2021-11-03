﻿using System.Collections;
using System.Collections.Generic;
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

    public List<JumpInfo> jumpSpaces;
    private static int _maxJumpArrows = 32;

    public AudioSource jumpSfx;

    CerberusMajor()
    {
        isCerberusMajor = true;
    }

    private void Start()
    {
        jumpSpaces = new List<JumpInfo>();
        _jumpArrows = new GameObject[_maxJumpArrows];
        for (int i = 0; i < _maxJumpArrows; i++)
        {
            _jumpArrows[i] = Instantiate(jumpArrowSource);
            _jumpArrows[i].gameObject.SetActive(false);
            jumpArrowSource.gameObject.SetActive(false);
        }
    }

    public override void StartMove()
    {
        base.StartMove();
        goalIsOnJumpPath = false;
    }

    public override void ProcessMoveInput()
    {
        base.ProcessMoveInput();
        var lastJumpSpacePosition = (jumpSpaces.Count > 0) ? jumpSpaces[jumpSpaces.Count - 1].position : position;
        if (input.specialHeld || (input.rightClicked && input.clickedCell != lastJumpSpacePosition))
        {
            if (input.upPressed || (input.clickedCell.x == lastJumpSpacePosition.x && input.clickedCell.y > lastJumpSpacePosition.y))
            {
                AddJumpSpace(Vector2Int.up, 90);
            }

            else if (input.downPressed || (input.clickedCell.x == lastJumpSpacePosition.x && input.clickedCell.y < lastJumpSpacePosition.y))
            {
                AddJumpSpace(Vector2Int.down, 270);
            }

            else if (input.rightPressed || (input.clickedCell.y == lastJumpSpacePosition.y && input.clickedCell.x > lastJumpSpacePosition.x))
            {
                AddJumpSpace(Vector2Int.right, 0);
            }

            else if (input.leftPressed || (input.clickedCell.y == lastJumpSpacePosition.y && input.clickedCell.x < lastJumpSpacePosition.x))
            {
                AddJumpSpace(Vector2Int.left, 180);
            }
        }
        else if (input.specialReleased || (input.rightClicked && input.clickedCell == lastJumpSpacePosition))
        {
            if (jumpSpaces.Count > 0)
            {
                puzzle.PushToUndoStack();
                
                JumpInfo[] jumpInfoCopy = new JumpInfo[jumpSpaces.Count];
                jumpSpaces.CopyTo(jumpInfoCopy);
                PlayAnimation(JumpAlongPath(jumpInfoCopy, AnimationUtility.jumpSpeed));
                
                // Travel across jump spaces
                foreach (var jumpInfo in jumpSpaces)
                {
                    Move(jumpInfo.position);
                }
                jumpSpaces.Clear();
                RenderJumpPath();

                DeclareDoneWithMove();
            }
        }
        else
        {
            if (input.upPressed || (input.clickedCell.x == position.x && input.clickedCell.y > position.y &&
                                    input.leftClicked))
            {
                BasicMove(Vector2Int.up);
                jumpSpaces.Clear();
                RenderJumpPath();
            }

            else if (input.downPressed || (input.clickedCell.x == position.x && input.clickedCell.y < position.y &&
                                           input.leftClicked))
            {
                BasicMove(Vector2Int.down);
                jumpSpaces.Clear();
                RenderJumpPath();
            }

            else if (input.rightPressed || (input.clickedCell.y == position.y && input.clickedCell.x > position.x &&
                                            input.leftClicked))
            {
                BasicMove(Vector2Int.right);
                jumpSpaces.Clear();
                RenderJumpPath();
            }

            else if (input.leftPressed || (input.clickedCell.y == position.y && input.clickedCell.x < position.x &&
                                           input.leftClicked))
            {
                BasicMove(Vector2Int.left);
                jumpSpaces.Clear();
                RenderJumpPath();
            }
        }

        if (input.undoPressed)
        {
            if (jumpSpaces.Count > 0)
            {
                jumpSpaces.Clear();
                RenderJumpPath();
            }
            else
            {
                manager.wantsToUndo = true;
            }
        }

        if (input.cycleCharacter)
        {
            jumpSpaces.Clear();
            RenderJumpPath();
            manager.wantsToCycleCharacter = true;
        }
    }

    private void AddJumpSpace(Vector2Int offset, float rotation)
    {
        var lastJumpPosition = (jumpSpaces.Count > 0) ? jumpSpaces[jumpSpaces.Count - 1].position : position;
        var jumpedOverSpace = lastJumpPosition + offset;
        var jumpedOverCell = puzzle.GetCell(jumpedOverSpace);
        var newJumpSpace = jumpedOverSpace + offset;
        var newJumpCell = puzzle.GetCell(newJumpSpace);
        // Check if user is "backing out"
        if (newJumpSpace == position)
        {
            jumpSpaces.Clear();
            RenderJumpPath();
            // Cerberus cannot possibly have goal in jump path.
            goalIsOnJumpPath = false;
            return;
        }

        // Check for entity to jump over.
        var canJump = (jumpedOverCell.GetJumpableEntity() || jumpedOverCell.floorTile.jumpable);
        if (canJump)
        {
            // Check for collision and if landable.
            var landableEntities = newJumpCell.GetLandableEntities();
            var newJumpCellLandableScore = newJumpCell.GetLandableScore();
            var canLand = newJumpCell.floorTile != null && (newJumpCellLandableScore >= 0);
            if (canLand)
            {
                var newJumpInfo = new JumpInfo(newJumpSpace, rotation);
                // Check if space is already in collection
                if (jumpSpaces.Contains(newJumpInfo))
                {
                    // Erase part of jump space path
                    var idxOfSpaceToRemove = jumpSpaces.IndexOf(newJumpInfo) + 1;
                    jumpSpaces.RemoveRange(idxOfSpaceToRemove, jumpSpaces.Count - idxOfSpaceToRemove);
                    RenderJumpPath();
                    // Cerberus cannot possibly have goal in jump path.
                    goalIsOnJumpPath = false;
                }
                else if(!goalIsOnJumpPath)
                {
                    // Add space to path if goal is not in path
                    jumpSpaces.Add(newJumpInfo);
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

    public void RenderJumpPath()
    {
        for (int i = 0; i < _jumpArrows.Length; i++)
        {
            var arrow = _jumpArrows[i];
            if (i >= jumpSpaces.Count)
            {
                arrow.SetActive(false);
            }
            else
            {
                arrow.SetActive(true);
                arrow.transform.position = puzzle.GetCellCenterWorld(jumpSpaces[i].position);
                arrow.transform.eulerAngles = new Vector3(0, 0, jumpSpaces[i].rotation);
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
            var distanceTraveled = 0f;
            var interpolation = 0f;
            var distanceToTravel = AnimationUtility.ApproximateLengthOfBezierCurve(A, B, C, D);

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