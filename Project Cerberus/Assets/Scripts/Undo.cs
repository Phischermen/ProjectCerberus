using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Undo : MonoBehaviour
{
    //checks if player has pressed undo
    public bool isUndone = false;

    List<Vector3> positions;

    protected PuzzleGameplayInput input;

    void Start()
    {
        input = FindObjectOfType<PuzzleGameplayInput>();
        positions = new List<Vector3>();
    }

    // Update is called once per frame
    void Update()
    {
        //PLACEHOLDER KEY
        //BOUND TO RIGHT SHIFT ON KEYBOARD AND CIRCLE ON GAMEPAD
        if (input.undoPressed)
            Undoing();
        if (!input.undoPressed)
            stopUndo();
        
    }

    void FixedUpdate()
    {
        if (isUndone)
            runItBack();
        else
            Record();
    }

    //moves positions back
    void runItBack()
    {
        transform.position = positions[0];
        positions.RemoveAt(0);
    }

    void Record()
    {
        positions.Insert(0, transform.position);
    }

    void Undoing()
    {
        isUndone = true;
    }

    void stopUndo()
    {
        isUndone = false;
    }
}
