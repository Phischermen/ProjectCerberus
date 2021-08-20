using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CerberusMajor : Cerberus
{
    [SerializeField] private GameObject jumpArrowSource;
    private GameObject[] _jumpArrows;


    private struct JumpInfo
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

        public JumpInfo(Vector2Int position, float rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }
    }

    private List<JumpInfo> _jumpSpaces;
    private static int _maxJumpArrows = 32;

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
                // Travel across jump spaces
                Move(_jumpSpaces[_jumpSpaces.Count - 1].position);
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
            return;
        }
        // Check for entity to jump over
        var canJump = (jumpedOverCell.puzzleEntities.Count > 0 || jumpedOverCell.floorTile.jumpable) &&
                      newJumpCell.floorTile != null;

        if (canJump)
        {
            // Check for collision and if landable
            var landableEntity = newJumpCell.GetLandableEntity();
            var canLand = (landableEntity != null) ||
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
                }
                else
                {
                    // Add space to path
                    _jumpSpaces.Add(newJumpInfo);
                    RenderJumpPath();
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
                arrow.transform.position = puzzle.tilemap.GetCellCenterWorld(new Vector3Int(_jumpSpaces[i].position.x,
                    _jumpSpaces[i].position.y, 0));
                arrow.transform.eulerAngles = new Vector3(0, 0, _jumpSpaces[i].rotation);
            }
        }
    }
}