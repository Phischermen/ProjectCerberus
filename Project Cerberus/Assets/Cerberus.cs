using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cerberus : PuzzleEntity
{
    // Start is called before the first frame update
    public Cerberus()
    {
        stopsPlayer = true;
        pushable = true;
    }

    public bool doneWithMove;

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ProcessMoveInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Debug.Log("Up!");
        }
    }

    public override void OnEnterCollisionWithEntity(PuzzleEntity other)
    {
        throw new System.NotImplementedException();
    }

    public override void OnExitCollisionWithEntity(PuzzleEntity other)
    {
        throw new System.NotImplementedException();
    }
}
