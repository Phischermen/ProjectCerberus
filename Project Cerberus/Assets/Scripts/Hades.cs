/*
 * Hades can chase the player using the "Sample Algorithm." Since this AI's primary purpose is to chase the player in 2
 * very specific scenes, it is limited in functionality. For example, only one target can be chased. The path needs to
 * be recalculated a lot too.  
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Hades : PuzzleEntity
{
    public PuzzleEntity entityToChase;
    private List<PuzzleContainer.LevelCell> _openList;

    public float chaseFrequency;
    public bool chaseEntityEnabled;
    private float _lastTimeChased;
    private bool _pathToTargetExists;
    private bool _onTopOfTarget;

    public UnityEvent onCatchTarget;
    public UnityEvent onBecomeTrapped;

    public class HadesStateData : StateData
    {
        public Hades hades;

        public HadesStateData(Hades hades)
        {
            this.hades = hades;
        }

        public override void Load()
        {
        }
    }

    protected Hades()
    {
        // Set properties here
        isHades = true;
        stopsPlayer = true;
        stopsBlock = true;
        pushableByJacksMultiPush = true;
    }

    protected override void Awake()
    {
        base.Awake();
        _lastTimeChased = Time.time;
        _openList = new List<PuzzleContainer.LevelCell>();
    }

    private new void Update()
    {
        base.Update();
        // Check if on top of chase target.
        if (position == entityToChase.position)
        {
            bool mayProceed = true;
            if (entityToChase is CerberusMajor cerberusMajor)
            {
                mayProceed = !cerberusMajor.isJumping;
            }
            if (!_onTopOfTarget && mayProceed)
            {
                _onTopOfTarget = true;
                onCatchTarget.Invoke();
            }
        }
        else
        {
            _onTopOfTarget = false;
        }

        // Check if time has come to move another square.
        if (chaseEntityEnabled && entityToChase != null && (Time.time - _lastTimeChased) > chaseFrequency &&
            inHole == false)
        {
            _lastTimeChased = Time.time;
            var notPreviouslyTrapped = _pathToTargetExists;
            var manhattanDistance = Mathf.Abs(position.x - entityToChase.position.x) +
                                    Mathf.Abs(position.y - entityToChase.position.y);
            // Determine if recalculation necessary.
            if (currentCell.spacesAwayFromChaseTarget == 0 ||
                manhattanDistance != currentCell.spacesAwayFromChaseTarget)
            {
                Debug.Log("Recalculated");
                SamplePathfind();
            }

            // Move
            var neighbor1 = puzzle.GetCell(position + Vector2Int.left);
            if (MoveHelper(neighbor1)) return;


            var neighbor2 = puzzle.GetCell(position + Vector2Int.right);
            if (MoveHelper(neighbor2)) return;


            var neighbor3 = puzzle.GetCell(position + Vector2Int.up);
            if (MoveHelper(neighbor3)) return;


            var neighbor4 = puzzle.GetCell(position + Vector2Int.down);
            if (!MoveHelper(neighbor4)) SamplePathfind();
            if (_pathToTargetExists == false && notPreviouslyTrapped)
            {
                onBecomeTrapped.Invoke();
            }
        }
    }

    private void SamplePathfind()
    {
        // Check if already on top of entity.
        _pathToTargetExists = position == entityToChase.position;
        if (_onTopOfTarget) return;
        _openList.Clear();
        _openList.Add(entityToChase.currentCell);
        PuzzleContainer.LevelCell.numberOfSearches += 1;
        var newSearchId = PuzzleContainer.LevelCell.numberOfSearches;
        entityToChase.currentCell.spacesAwayFromChaseTarget = 0;
        entityToChase.currentCell.searchId = newSearchId;
        for (int i = 0; i < _openList.Count; i++)
        {
            var currentElement = _openList[i];
            // 1st Successor (North)
            var successor1 = puzzle.GetCell(currentElement.position + Vector2Int.up);
            SampleHelper(currentElement, successor1, newSearchId);
            // 2nd Successor (East)
            var successor2 = puzzle.GetCell(currentElement.position + Vector2Int.right);
            SampleHelper(currentElement, successor2, newSearchId);
            // 3rd Successor (West)
            var successor3 = puzzle.GetCell(currentElement.position + Vector2Int.left);
            SampleHelper(currentElement, successor3, newSearchId);
            // 4th Successor (South)
            var successor4 = puzzle.GetCell(currentElement.position + Vector2Int.down);
            SampleHelper(currentElement, successor4, newSearchId);
        }
    }

    private void SampleHelper(PuzzleContainer.LevelCell currentElement, PuzzleContainer.LevelCell successor,
        int newSearchId)
    {
        if (!successor.floorTile.stopsHades && successor.GetLandableScore() >= 0)
        {
            if (successor.searchId != newSearchId)
            {
                successor.searchId = newSearchId;
                successor.spacesAwayFromChaseTarget = currentElement.spacesAwayFromChaseTarget + 1;
                _openList.Add(successor);
                if (successor.position == position)
                {
                    _pathToTargetExists = true;
                }
            }
        }
        else
        {
            successor.searchId = newSearchId;
            successor.spacesAwayFromChaseTarget = Int32.MaxValue;
        }
    }

    public bool MoveHelper(PuzzleContainer.LevelCell neighbor)
    {
        if (neighbor.spacesAwayFromChaseTarget < currentCell.spacesAwayFromChaseTarget &&
            neighbor.GetLandableScore() >= 0)
        {
            PlayAnimation(SlideToDestination(neighbor.position, AnimationUtility.basicMoveAndPushSpeed));
            Move(neighbor.position);
            return true;
        }

        return false;
    }

    public void MoveForCutscene(Vector2Int offset)
    {
        Move(position + offset);
        PlayAnimation(SlideToDestination(position, AnimationUtility.basicMoveAndPushSpeed));
    }

    public override StateData GetUndoData()
    {
        var undoData = new HadesStateData(this);
        return undoData;
    }
}