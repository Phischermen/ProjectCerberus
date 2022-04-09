/*
 * PuzzleGameplayInput is responsible for gathering input from the player every frame. It supports keyboard, gamepad,
 * and mouse.
 */

using UnityEngine;
using UnityEngine.InputSystem;

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
        toggleFixedCameraMode,
        cycleCharacter,
        cycleCharacterForward,
        cycleCharacterBackward,
        cycleCharacter0,
        cycleCharacter1,
        cycleCharacter2,
        leftClicked,
        rightClicked,
        dialogueDismissed,
        pause;

    [HideInInspector] public Vector2Int clickedCell;
    [HideInInspector] public Cerberus clickedCerberus;

    private PuzzleContainer _puzzleContainer;
    private Cerberus[] allCerberus;
    private Camera mainCamera;

    private void Awake()
    {
        _puzzleContainer = FindObjectOfType<PuzzleContainer>();
        allCerberus = FindObjectsOfType<Cerberus>();
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

            toggleFixedCameraMode = gamepad.triangleButton.wasPressedThisFrame;

            cycleCharacterBackward = gamepad.leftShoulder.wasPressedThisFrame;
            cycleCharacterForward = gamepad.rightShoulder.wasPressedThisFrame;

            pause = gamepad.startButton.wasPressedThisFrame;
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

            undoPressed = undoPressed || keyboard.uKey.wasPressedThisFrame;
            resetPressed = resetPressed || keyboard.rKey.wasPressedThisFrame;

            toggleFixedCameraMode = toggleFixedCameraMode || keyboard.spaceKey.wasPressedThisFrame;

            cycleCharacterForward = cycleCharacterForward || keyboard.tabKey.wasPressedThisFrame;
            cycleCharacter0 = cycleCharacter0 || keyboard.digit1Key.wasPressedThisFrame;
            cycleCharacter1 = cycleCharacter1 || keyboard.digit2Key.wasPressedThisFrame;
            cycleCharacter2 = cycleCharacter2 || keyboard.digit3Key.wasPressedThisFrame;

            pause = pause || keyboard.escapeKey.wasPressedThisFrame;
        }

        if (mouse != null)
        {
            ProcessMouse(mouse);
        }

        cycleCharacter = cycleCharacter || cycleCharacter0 || cycleCharacter1 || cycleCharacter2 ||
                         cycleCharacterForward || cycleCharacterBackward;
        dialogueDismissed = specialPressed || undoPressed || mergeOrSplit || leftClicked;
    }

    private void ProcessMouse(Mouse mouse)
    {
        var mousePosition = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        leftClicked = mouse.leftButton.wasPressedThisFrame;
        rightClicked = mouse.rightButton.wasPressedThisFrame;
        if (leftClicked || rightClicked)
        {
            // Convert mouse position to grid cell.
            var clickedCellV3 =
                _puzzleContainer.tilemap.layoutGrid.WorldToCell(new Vector3(mousePosition.x, mousePosition.y, 0f));
            clickedCell = new Vector2Int(clickedCellV3.x, clickedCellV3.y);
        }

        // Check if a cerberus was clicked.
        if (leftClicked)
        {
            foreach (var cerberus in allCerberus)
            {
                if (Vector2.Distance(cerberus.transform.position, mousePosition) < 0.25f)
                {
                    clickedCerberus = cerberus;
                    cycleCharacter = true;
                    if (cerberus.collisionsEnabled == false)
                    {
                        mergeOrSplit = true;
                    }

                    // Prevent cerberus from moving or using ability.
                    leftClicked = false;
                    rightClicked = false;
                }
            }
        }
    }

    public void ClearInput()
    {
        clickedCell = Vector2Int.zero;
        clickedCerberus = null;
        leftPressed = rightPressed = upPressed = downPressed = leftReleased = rightReleased = upReleased =
            downReleased = specialPressed = specialHeld = specialReleased = mergeOrSplit =
                undoPressed = resetPressed = toggleFixedCameraMode = leftClicked = rightClicked = cycleCharacter =
                    cycleCharacterForward = cycleCharacterBackward = cycleCharacter0 = cycleCharacter1 =
                        cycleCharacter2 = dialogueDismissed = pause = false;
    }
}