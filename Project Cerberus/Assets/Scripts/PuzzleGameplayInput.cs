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
        mergeOrSplit,
        undoPressed,
        resetPressed,
        cycleCharacter,
        cycleCharacterForward,
        cycleCharacterBackward,
        cycleCharacter0,
        cycleCharacter1,
        cycleCharacter2;

    public Vector2 clickedCell;
    public bool wasLeftClick;
    
    private PuzzleContainer _puzzleContainer;
    private Camera mainCamera;

    private void Awake()
    {
        _puzzleContainer = FindObjectOfType<PuzzleContainer>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        Gamepad gamepad = Gamepad.current;
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;
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

            mergeOrSplit = gamepad.squareButton.wasPressedThisFrame;

            undoPressed = gamepad.circleButton.wasPressedThisFrame;
            resetPressed = gamepad.leftTrigger.wasPressedThisFrame;

            cycleCharacterBackward = gamepad.leftShoulder.wasPressedThisFrame;
            cycleCharacterForward = gamepad.rightShoulder.wasPressedThisFrame;
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

            mergeOrSplit = mergeOrSplit || keyboard.leftCtrlKey.wasPressedThisFrame;

            undoPressed = undoPressed || keyboard.rightShiftKey.wasPressedThisFrame;
            resetPressed = resetPressed || keyboard.rKey.wasPressedThisFrame;

            cycleCharacterForward = cycleCharacterForward || keyboard.tabKey.wasPressedThisFrame;
            cycleCharacter0 = cycleCharacter0 || keyboard.digit1Key.wasPressedThisFrame;
            cycleCharacter1 = cycleCharacter1 || keyboard.digit2Key.wasPressedThisFrame;
            cycleCharacter2 = cycleCharacter2 || keyboard.digit3Key.wasPressedThisFrame;
        }

        if (mouse != null)
        {
            ProcessMouse(mouse);
        }
        cycleCharacter = cycleCharacter0 || cycleCharacter1 || cycleCharacter2 || cycleCharacterForward ||
                         cycleCharacterBackward;
    }

    private void ProcessMouse(Mouse mouse)
    {
        var mousePosition = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        var mouseCoord = new Vector2Int((int) mousePosition.x, (int) mousePosition.y);
        // TODO get levelCell coordinate that was clicked. Also get what mouse button was clicked.
        //_puzzleContainer.tilemap.layoutGrid.WorldToCell(,, 0f);
        // Convert mouse position to grid cell.
        var inBounds = _puzzleContainer.InBounds(mouseCoord);
    }

    public void ClearInput()
    {
        // TODO Reset mouse click
        leftPressed = rightPressed = upPressed = downPressed = leftReleased = rightReleased = upReleased =
            downReleased = specialPressed = specialHeld = specialReleased = mergeOrSplit =
                undoPressed = resetPressed = cycleCharacter = cycleCharacterForward =
                    cycleCharacterBackward = cycleCharacter0 = cycleCharacter1 = cycleCharacter2 = false;
    }
}