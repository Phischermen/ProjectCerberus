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

    public class HintSpaceUndoData : UndoData
    {
        public HintSpace hintSpace;
        private bool _visited;

        public HintSpaceUndoData(HintSpace hintSpace, bool visited)
        {
            this.hintSpace = hintSpace;
            this._visited = visited;
        }

        public override void Load()
        {
            hintSpace._visited = _visited;
            if (_visited)
            {
                hintSpace.SetFieldsToVisitedPreset();
            }
            else
            {
                hintSpace.SetFieldsToNotVisitedPreset();
            }
        }
    }

    protected HintSpace()
    {
        isStatic = true;
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
            hint.gameObject.SetActive(true);
            _visited = true;
            SetFieldsToVisitedPreset();
        }
    }

    public override void OnExitCollisionWithEntity(PuzzleEntity other)
    {
        // Hide hint
        hint.gameObject.SetActive(false);
    }

    private void SetFieldsToVisitedPreset()
    {
        spriteRenderer.sprite = visitedSprite;
    }

    private void SetFieldsToNotVisitedPreset()
    {
        spriteRenderer.sprite = _unvisitedSprite;
    }

    public override UndoData GetUndoData()
    {
        var undoData = new HintSpaceUndoData(this, _visited);
        return undoData;
    }
}