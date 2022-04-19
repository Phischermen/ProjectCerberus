/*
* HintSpace shows a tutorial dialogue when it is stepped on, and hides it when the player steps off.
*/

using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class HintSpace : PuzzleEntity
{
    public GameObject hintPrefab;
    public RectTransform hint;
    public Sprite visitedSprite;
    private Sprite _unvisitedSprite;
    private bool _visited;
    private bool _currentlyOccupied;

    public class HintSpaceStateData : StateData
    {
        public HintSpace hintSpace;
        private bool _visited => booleans[0];
        private bool _currentlyOccupied => booleans[1];

        public HintSpaceStateData(HintSpace hintSpace, bool visited, bool currentlyOccupied)
        {
            this.hintSpace = hintSpace;
            booleans[0] = visited;
            booleans[1] = currentlyOccupied;
        }

        public override void Load()
        {
            hintSpace._visited = _visited;
            hintSpace._currentlyOccupied = _currentlyOccupied;
            if (_visited)
            {
                hintSpace.SetFieldsToVisitedPreset();
            }
            else
            {
                hintSpace.SetFieldsToNotVisitedPreset();
            }

            if (_currentlyOccupied)
            {
                hintSpace.hint.gameObject.SetActive(true);
            }
            else
            {
                hintSpace.hint.gameObject.SetActive(false);
            }
        }
    }

    protected HintSpace()
    {
        isStatic = true;
        ignoresSwitch = true;
        entityRules =
            "HintSpace shows a tutorial dialogue when it is stepped on, and hides it when the player steps off.";
    }

    private void Start()
    {
        if (hint != null && Application.isPlaying)
        {
            hint.gameObject.SetActive(false);
        }

        _unvisitedSprite = spriteRenderer.sprite;
    }

#if UNITY_EDITOR
    protected override void Awake()
    {
        base.Awake();
        if (hint == null)
        {
            // Find Canvas
            var hintCanvas = FindObjectOfType<PuzzleContainer>().GetComponentInChildren<Canvas>();
            hint = ((GameObject) PrefabUtility.InstantiatePrefab(hintPrefab, hintCanvas.transform))
                .GetComponent<RectTransform>();
        }
    }
#endif

    public override void OnEnterCollisionWithEntity(PuzzleEntity other)
    {
        if (other is Cerberus)
        {
            // Show hint
            _currentlyOccupied = true;
            hint.gameObject.SetActive(true);
            _visited = true;
            SetFieldsToVisitedPreset();
        }
    }

    public override void OnExitCollisionWithEntity(PuzzleEntity other)
    {
        if (other is Cerberus)
        {
            // Hide hint
            _currentlyOccupied = false;
            hint.gameObject.SetActive(false);
        }
    }

    private void SetFieldsToVisitedPreset()
    {
        spriteRenderer.sprite = visitedSprite;
    }

    private void SetFieldsToNotVisitedPreset()
    {
        spriteRenderer.sprite = _unvisitedSprite;
    }

    public override StateData GetUndoData()
    {
        var undoData = new HintSpaceStateData(this, _visited, _currentlyOccupied);
        return undoData;
    }
}