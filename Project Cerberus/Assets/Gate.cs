using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : PuzzleEntity
{
    private bool wantsToClose;
    [ShowInTileInspector] public bool open;
    [SerializeField] private Sprite openSprite;
    [SerializeField] private Sprite closeSprite;
    
    public Gate()
    {
        // Initialized in closed state
        SetFieldsToClosedPreset();
    }

    private void Update()
    {
        if (wantsToClose)
        {
            var myCell = puzzle.GetCell(position);
            if (!CollidesWithAny(myCell.puzzleEntities))
            {
                CloseGate();
            }
        }
    }

    public void OpenGate()
    {
        wantsToClose = false;
        GetComponent<SpriteRenderer>().sprite = openSprite;
        SetFieldsToOpenPreset();
    }

    public void RequestCloseGate()
    {
        wantsToClose = true;
    }

    private void CloseGate()
    {
        wantsToClose = false;
        GetComponent<SpriteRenderer>().sprite = closeSprite;
        SetFieldsToClosedPreset();
    }

    private void SetFieldsToOpenPreset()
    {
        open = true;
        stopsBlock = false;
        stopsPlayer = false;
        landable = true;
    }

    private void SetFieldsToClosedPreset()
    {
        open = false;
        stopsBlock = true;
        stopsPlayer = true;
        landable = false;
    }
}