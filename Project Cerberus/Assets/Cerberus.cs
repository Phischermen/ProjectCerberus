using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cerberus : PuzzleEntity
{
    // Start is called before the first frame update
    public Cerberus()
    {
        isPlayer = true;
        stopsPlayer = true;
        stopsBlock = true;
        pushable = true;
    }

    public bool doneWithMove;
    public bool _verticalAxisReleased = true;
    public bool _horizontalAxisReleased = true;

    public int _verticalAxisJustHeld = 0;
    public int _horizontalAxisJustHeld = 0;

    public int _verticalAxisJustReleased = 0;
    public int _horizontalAxisJustReleased = 0;

    // Update is called once per frame
    void Update()
    {
    }

    public virtual void ProcessMoveInput()
    {
        _verticalAxisJustHeld = _horizontalAxisJustHeld = 0;
        _verticalAxisJustReleased = _horizontalAxisJustReleased = 0;
        if (_verticalAxisReleased)
        {
            if (Input.GetAxis("Vertical") > 0.5)
            {
                _verticalAxisJustHeld = 1;
                _verticalAxisReleased = false;
            }
            else if (Input.GetAxis("Vertical") < -0.5)
            {
                _verticalAxisJustHeld = -1;
                _verticalAxisReleased = false;
            }
        }
        else if (Mathf.Abs(Input.GetAxis("Vertical")) < 0.5)
        {
            if (Input.GetAxis("Vertical") > 0)
            {
                _verticalAxisJustReleased = 1;
                _verticalAxisReleased = true;
            }
            else if (Input.GetAxis("Vertical") < 0)
            {
                _verticalAxisJustReleased = -1;
                _verticalAxisReleased = true;
            }
        }

        if (_horizontalAxisReleased)
        {
            if (Input.GetAxis("Horizontal") > 0.5)
            {
                _horizontalAxisJustHeld = 1;
                _horizontalAxisReleased = false;
            }
            else if (Input.GetAxis("Horizontal") < -0.5)
            {
                _horizontalAxisJustHeld = -1;
                _horizontalAxisReleased = false;
            }
        }
        else if (Mathf.Abs(Input.GetAxis("Horizontal")) < 0.5)
        {
            if (Input.GetAxis("Horizontal") > 0)
            {
                _horizontalAxisJustReleased = 1;
                _horizontalAxisReleased = true;
            }
            else if (Input.GetAxis("Horizontal") < 0)
            {
                _horizontalAxisJustReleased = -1;
                _horizontalAxisReleased = true;
            }
        }
    }
}