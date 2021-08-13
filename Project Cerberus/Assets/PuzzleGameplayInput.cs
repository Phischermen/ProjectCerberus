using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PuzzleGameplayInput : MonoBehaviour
{
    [HideInInspector] public bool leftPressed,
        rightPressed,
        upPressed,
        downPressed,
        leftReleased,
        rightReleased,
        upReleased,
        downReleased,
        specialPressed,
        specialHeld,
        specialReleased,
        skipMove,
        mergeOrSplit,
        undoPressed;

    private void Update()
    {
        Gamepad gamepad = Gamepad.current;
        Keyboard keyboard = Keyboard.current;
        ClearInput();
        if (gamepad != null)
        {
            leftPressed = gamepad.dpad.left.wasPressedThisFrame || gamepad.leftStick.left.wasPressedThisFrame;
            rightPressed = gamepad.dpad.right.wasPressedThisFrame || gamepad.leftStick.right.wasPressedThisFrame;
            upPressed = gamepad.dpad.up.wasPressedThisFrame || gamepad.leftStick.up.wasPressedThisFrame;
            downPressed = gamepad.dpad.down.wasPressedThisFrame || gamepad.leftStick.down.wasPressedThisFrame;

            leftReleased = gamepad.dpad.left.wasReleasedThisFrame || gamepad.leftStick.left.wasReleasedThisFrame;
            rightReleased = gamepad.dpad.right.wasReleasedThisFrame || gamepad.leftStick.right.wasReleasedThisFrame;
            upReleased = gamepad.dpad.up.wasReleasedThisFrame || gamepad.leftStick.up.wasReleasedThisFrame;
            downReleased = gamepad.dpad.down.wasReleasedThisFrame || gamepad.leftStick.down.wasReleasedThisFrame;

            specialPressed = gamepad.crossButton.wasPressedThisFrame;
            specialHeld = gamepad.crossButton.isPressed;
            specialReleased = gamepad.crossButton.wasReleasedThisFrame;

            skipMove = gamepad.triangleButton.wasPressedThisFrame;
            mergeOrSplit = gamepad.squareButton.wasPressedThisFrame;

            undoPressed = gamepad.circleButton.wasPressedThisFrame;
        }

        if (keyboard != null)
        {
            leftPressed = leftPressed || keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame;
            rightPressed = rightPressed || keyboard.dKey.wasPressedThisFrame ||
                           keyboard.rightArrowKey.wasPressedThisFrame;
            upPressed = upPressed || keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame;
            downPressed = downPressed || keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame;

            leftReleased = leftReleased || keyboard.aKey.wasReleasedThisFrame ||
                           keyboard.leftArrowKey.wasReleasedThisFrame;
            rightReleased = rightReleased || keyboard.dKey.wasReleasedThisFrame ||
                            keyboard.rightArrowKey.wasReleasedThisFrame;
            upReleased = upReleased || keyboard.wKey.wasReleasedThisFrame || keyboard.upArrowKey.wasReleasedThisFrame;
            downReleased = downReleased || keyboard.sKey.wasReleasedThisFrame ||
                           keyboard.downArrowKey.wasReleasedThisFrame;

            specialPressed = specialPressed || keyboard.leftShiftKey.wasPressedThisFrame;
            specialHeld = specialHeld || keyboard.leftShiftKey.isPressed;
            specialReleased = specialReleased || keyboard.leftShiftKey.wasReleasedThisFrame;

            skipMove = skipMove || keyboard.enterKey.wasPressedThisFrame;
            mergeOrSplit = mergeOrSplit || keyboard.leftCtrlKey.wasPressedThisFrame;

            undoPressed = undoPressed || keyboard.rightShiftKey.wasPressedThisFrame;
        }
    }

    public void ClearInput()
    {
        leftPressed = rightPressed = upPressed = downPressed = leftReleased = rightReleased =
            upReleased = downReleased = specialPressed =
                specialHeld = specialReleased = skipMove = mergeOrSplit = undoPressed = false;
    }
}