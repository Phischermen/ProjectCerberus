/*
 * Game manager searches the scene on Start() for critical components, such as JKL, and manages their split, join, undo,
 * and cycle abilities. Game manager also defines certain constraints for the level such as time limit and max moves
 * until star loss. Game manager is also responsible for transitioning from gameplay to win/lose states, and loading the
 * next scene.
 */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour, IUndoable
{
    class GameManagerUndoData : UndoData
    {
        private GameManager _gameManager;
        private Cerberus _currentCerberus;

        private int _move;
        private float _timer;

        private bool _joinSplitEnabled;
        private bool _cerberusFormed;

        private bool _collectedStar;

        public GameManagerUndoData(GameManager gameManager, int move, float timer, Cerberus currentCerberus,
            bool cerberusFormed, bool joinSplitEnabled, bool collectedStar)
        {
            _gameManager = gameManager;
            _currentCerberus = currentCerberus;
            _move = move;
            _timer = timer;
            _cerberusFormed = cerberusFormed;
            _joinSplitEnabled = joinSplitEnabled;
            _collectedStar = collectedStar;
        }

        public override void Load()
        {
            // Start the move of the newly controlled Cerberus
            _gameManager.currentCerberus = _currentCerberus;
            _currentCerberus.StartMove();
            _gameManager.move = _move;
            _gameManager.timer = _timer;
            // Stop timer if first move. Otherwise run timer.
            if (_move == 0)
            {
                _gameManager._timerRunning = false;
            }
            else
            {
                _gameManager._timerRunning = true;
            }

            _gameManager.cerberusFormed = _cerberusFormed;
            _gameManager.joinAndSplitEnabled = _joinSplitEnabled;
            // Repopulate available cerberus
            _gameManager.RepopulateAvailableCerberus(!_cerberusFormed);
            _gameManager.collectedStar = _collectedStar;
        }
    }

#if DEBUG
    private void OnGUI()
    {
        if (GUI.Button(new Rect(200, 0, 200, 20), "Skip level"))
        {
            ProceedToNextLevel();
        }
    }
#endif

    public static LevelSequence levelSequence;
    public static int currentLevel = -1;

    [HideInInspector] public bool infiniteParTime = true;
    [HideInInspector] public float parTime = 1f;

    private bool _timerRunning;

    public float timer { get; protected set; }

    public int move { get; protected set; }
    [HideInInspector] public bool infinteMovesTilStarLoss = true;

    [FormerlySerializedAs("maxMovesUntilStarLoss")] [HideInInspector]
    public int maxMovesBeforeStarLoss;

    public bool startInFixedCameraMode = true;

    [SerializeField] private GameObject _uiPrefab;
    [SerializeField] private GameObject _gameOverEndCard;
    [SerializeField] private GameObject _victoryEndCard;

    private Laguna _laguna;
    private Jack _jack;
    private Kahuna _kahuna;
    private CerberusMajor _cerberusMajor;
    public List<Cerberus> availableCerberus { get; protected set; }
    [HideInInspector] public Cerberus currentCerberus;
    private PuzzleContainer _puzzleContainer;
    private PuzzleGameplayInput _input;
    private PuzzleCameraController _cameraController;

    [HideInInspector] public bool joinAndSplitEnabled;
    public bool cerberusFormed { get; protected set; }

    [HideInInspector] public bool wantsToJoin;
    [HideInInspector] public bool wantsToSplit;
    [HideInInspector] public bool wantsToCycleCharacter;
    [HideInInspector] public bool wantsToUndo;

    private int _cerberusThatMustReachGoal;
    [HideInInspector] public bool collectedStar;

    [HideInInspector] public bool gameplayEnabled;

    [HideInInspector] public bool gameOverEndCardDisplayed;

    void Awake()
    {
        // Create UI
        Instantiate(_uiPrefab);
        // Load Level Sequence and get current world and level
        if (levelSequence == null)
        {
            levelSequence = Resources.Load<CustomProjectSettings>(CustomProjectSettings.resourcePath)
                .mainLevelSequence;
            currentLevel = levelSequence.FindCurrentLevelSequence(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void Start()
    {
        // Get objects
        _puzzleContainer = FindObjectOfType<PuzzleContainer>();
        _input = FindObjectOfType<PuzzleGameplayInput>();
        _cameraController = FindObjectOfType<PuzzleCameraController>();
        if (_cameraController == null)
        {
            _cameraController = GameObject.FindGameObjectWithTag("MainCamera").AddComponent<PuzzleCameraController>();
        }
        else
        {
            NZ.NotifyZach("Main Camera has a redundant CameraController component. Remove it NOW or you're fired.");
        }

        _jack = FindObjectOfType<Jack>();
        _kahuna = FindObjectOfType<Kahuna>();
        _laguna = FindObjectOfType<Laguna>();
        _cerberusMajor = FindObjectOfType<CerberusMajor>();

        // Initialize availableCerberus
        availableCerberus = new List<Cerberus>();
        if (_jack)
        {
            availableCerberus.Add(_jack);
            _cerberusThatMustReachGoal += 1;
        }

        if (_kahuna)
        {
            availableCerberus.Add(_kahuna);
            _cerberusThatMustReachGoal += 1;
        }

        if (_laguna)
        {
            availableCerberus.Add(_laguna);
            _cerberusThatMustReachGoal += 1;
        }

        currentCerberus = availableCerberus[0];
        
        // Setup camera.
        _cameraController.SetCameraMode(startInFixedCameraMode
            ? PuzzleCameraController.CameraMode.FixedPointMode
            : PuzzleCameraController.CameraMode.ScrollingMode);
        _cameraController.GotoDesiredPositionAndSize();

        // Set initial gameplay variables
        if (_cerberusMajor)
        {
            // Player may only merge or split if cerberus major is in scene
            joinAndSplitEnabled = true;
            // Cerberus Major is inactive by default
            _cerberusMajor.SetDisableCollsionAndShowPentagramMarker(true);
        }

        timer = 0;
        _timerRunning = false;
        gameplayEnabled = true;
    }

    void Update()
    {
        // Process movement of currently controlled cerberus if gameplay is enabled.
        if (gameplayEnabled)
        {
            var nextMoveNeedsToStart = false;
            currentCerberus.ProcessMoveInput();
            // Check if cerberus made their move
            if (currentCerberus.doneWithMove)
            {
                // Start timer
                _timerRunning = true;
                // Check how many of available cerberus are on goal.
                var availableCerberusOnGoal = 0;
                foreach (var cerberus in availableCerberus)
                {
                    if (cerberus.onTopOfGoal)
                    {
                        // Cerberus Major represents JKL combined, hence why we increment by three if Cerberus Major
                        // is on goal.
                        availableCerberusOnGoal += (cerberus.isCerberusMajor) ? 3 : 1;
                    }
                }

                if (availableCerberusOnGoal == _cerberusThatMustReachGoal)
                {
                    // Victory! Winning!
                    // Play victory animation
                    EndGameWithSuccessStatus();
                }

                if (!joinAndSplitEnabled)
                {
                    // Increment move normally
                    move += 1;
                }
                else
                {
                    // Handle request to split/join
                    if (wantsToJoin)
                    {
                        wantsToJoin = false;
                        // Set collisionsEnabled in preparation for collision test. JKL collisions are disabled in case
                        // they happen to be standing on top of Cerberus. Callbacks are not invoked, for this is meant
                        // to be a test.
                        _cerberusMajor.SetCollisionsEnabled(true, invokeCallbacks: false);
                        _jack.SetCollisionsEnabled(false, invokeCallbacks: false);
                        _kahuna.SetCollisionsEnabled(false, invokeCallbacks: false);
                        _laguna.SetCollisionsEnabled(false, invokeCallbacks: false);
                        // Check for collision at CerberusMajor
                        var joinBlocked = _cerberusMajor.CollidesWith(_cerberusMajor.currentCell) &&
                                          _cerberusMajor.currentCell.GetLandableScore() <= 0;

                        // Reset collisionsEnabled, so that FormCerberusMajor() functions properly.
                        _cerberusMajor.SetCollisionsEnabled(false, invokeCallbacks: false);
                        _jack.SetCollisionsEnabled(true, invokeCallbacks: false);
                        _kahuna.SetCollisionsEnabled(true, invokeCallbacks: false);
                        _laguna.SetCollisionsEnabled(true, invokeCallbacks: false);

                        if (!joinBlocked)
                        {
                            // Join request granted
                            _puzzleContainer.PushToUndoStack();
                            FormCerberusMajor();
                        }
                    }
                    else if (wantsToSplit)
                    {
                        wantsToSplit = false;
                        // Check for collision at JKL
                        _cerberusMajor.SetCollisionsEnabled(false, invokeCallbacks: false);
                        _jack.SetCollisionsEnabled(true, invokeCallbacks: false);
                        _kahuna.SetCollisionsEnabled(true, invokeCallbacks: false);
                        _laguna.SetCollisionsEnabled(true, invokeCallbacks: false);
                        var splitBlocked = _jack.CollidesWith(_jack.currentCell) ||
                                           _kahuna.CollidesWith(_kahuna.currentCell) ||
                                           _laguna.CollidesWith(_laguna.currentCell);

                        // Reset collisionEnabled, so that SplitCerberusMajor() functions properly.
                        _cerberusMajor.SetCollisionsEnabled(true, invokeCallbacks: false);
                        _jack.SetCollisionsEnabled(false, invokeCallbacks: false);
                        _kahuna.SetCollisionsEnabled(false, invokeCallbacks: false);
                        _laguna.SetCollisionsEnabled(false, invokeCallbacks: false);

                        if (!splitBlocked)
                        {
                            // Split request granted
                            _puzzleContainer.PushToUndoStack();
                            SplitCerberusMajor();
                        }
                    }
                    // Increment move if the move was not joining or splitting.
                    else
                    {
                        move += 1;
                    }
                }

                // Command PuzzleContainer to process entities in response to this move
                _puzzleContainer.ProcessEntitiesInResponseToPlayerMove();
                // Start the next move with currentCerberus
                nextMoveNeedsToStart = true;
            }

            // Handle request to undo
            if (wantsToUndo)
            {
                wantsToUndo = false;
                _puzzleContainer.UndoLastMove();
            }

            // Handle request to cycle character
            if (wantsToCycleCharacter && !cerberusFormed)
            {
                wantsToCycleCharacter = false;
                // Sort availableCerberus, from north-west most to south-east most.
                var availableCerberusSorted = availableCerberus.OrderBy(CompareCerberusByPosition).ToList();
                // Get index of currentCerberus after sorting the list.
                var currentIdx = availableCerberusSorted.IndexOf(currentCerberus);
                if (_input.cycleCharacterForward)
                {
                    // Switch to Cerberus south-east of current Cerberus.
                    var idx = (currentIdx + 1) % availableCerberusSorted.Count;
                    currentCerberus = availableCerberusSorted[idx];
                }
                else if (_input.cycleCharacterBackward)
                {
                    // Switch to Cerberus north-west of current Cerberus.
                    // Note: To avoid getting a negative index from % operator, I add the array length to currentIndex. 
                    var idx = ((currentIdx - 1 + availableCerberusSorted.Count) % availableCerberusSorted.Count);
                    currentCerberus = availableCerberusSorted[idx];
                }
                else if (_input.cycleCharacter0 && availableCerberus.Count > 0)
                {
                    // Switch to Cerberus at far north-west.
                    currentCerberus = availableCerberus[0];
                }
                else if (_input.cycleCharacter1 && availableCerberus.Count > 1)
                {
                    // Switch to the Cerberus between Cerberus.
                    currentCerberus = availableCerberus[1];
                }
                else if (_input.cycleCharacter2 && availableCerberus.Count > 2)
                {
                    // Switch to Cerberus at far south-east.
                    currentCerberus = availableCerberus[2];
                }
                else if (_input.clickedCerberus != null)
                {
                    currentCerberus = _input.clickedCerberus;
                }

                nextMoveNeedsToStart = true;
            }

            // Handle request for switching camera mode
            if (_input.toggleFixedCameraMode)
            {
                _cameraController.ToggleCameraMode();
            }

            if (nextMoveNeedsToStart)
            {
                currentCerberus.StartMove();
            }
        }

        // Run timer
        if (_timerRunning)
        {
            timer += Time.deltaTime;
        }
    }

    // Gameover
    public void EndGameWithFailureStatus()
    {
        // Disable gameplay
        gameplayEnabled = false;
        // Stop timer
        _timerRunning = false;
        if (!gameOverEndCardDisplayed)
        {
            gameOverEndCardDisplayed = true;
            // Display game over end card.
            Instantiate(_gameOverEndCard);
        }
    }

    public void EndGameWithSuccessStatus()
    {
        // Disable gameplay.
        gameplayEnabled = false;
        // Stop timer
        _timerRunning = false;
        // Display victory end card.
        Instantiate(_victoryEndCard);
    }

    // Comparison Delegate
    private int CompareCerberusByPosition(Cerberus a)
    {
        return a.position.x * PuzzleContainer.maxLevelWidth + (PuzzleContainer.maxLevelWidth - a.position.y);
    }

    // Undo
    public UndoData GetUndoData()
    {
        var undoData = new GameManagerUndoData(this, move, timer, currentCerberus, cerberusFormed, joinAndSplitEnabled,
            collectedStar);
        return undoData;
    }

    // Merge and split Management
    public void FormCerberusMajor()
    {
        cerberusFormed = true;
        // Play sound effects
        _cerberusMajor.PlaySfx(_cerberusMajor.mergeSfx);
        // Stop animations for JKL
        _jack.FinishCurrentAnimation();
        _kahuna.FinishCurrentAnimation();
        _laguna.FinishCurrentAnimation();
        // Activate Cerberus Major and deactivate JKL
        _cerberusMajor.SetDisableCollsionAndShowPentagramMarker(false);
        _jack.SetDisableCollsionAndShowPentagramMarker(true);
        _kahuna.SetDisableCollsionAndShowPentagramMarker(true);
        _laguna.SetDisableCollsionAndShowPentagramMarker(true);
        // Repopulate availableCerberus
        availableCerberus.Clear();
        availableCerberus.Add(_cerberusMajor);
        currentCerberus = _cerberusMajor;
    }

    public void SplitCerberusMajor()
    {
        cerberusFormed = false;
        // Play sound effects
        _cerberusMajor.PlaySfx(_cerberusMajor.splitSfx);
        // Stop animation for Cerberus Major
        _cerberusMajor.FinishCurrentAnimation();
        // Activate JKL and deactivate Cerberus Major
        _cerberusMajor.SetDisableCollsionAndShowPentagramMarker(true);
        _jack.SetDisableCollsionAndShowPentagramMarker(false);
        _kahuna.SetDisableCollsionAndShowPentagramMarker(false);
        _laguna.SetDisableCollsionAndShowPentagramMarker(false);
        // Repopulate availableCerberus
        availableCerberus.Clear();
        availableCerberus.Add(_jack);
        availableCerberus.Add(_kahuna);
        availableCerberus.Add(_laguna);
        if (_input.clickedCerberus != null)
        {
            currentCerberus = _input.clickedCerberus;
        }
        else
        {
            currentCerberus = _jack;
        }
    }

    // Available Cerberus Management
    // ReSharper disable once InconsistentNaming
    public void RepopulateAvailableCerberus(bool withJKL = true)
    {
        availableCerberus.Clear();
        if (withJKL)
        {
            if (_jack)
            {
                availableCerberus.Add(_jack);
            }

            if (_kahuna)
            {
                availableCerberus.Add(_kahuna);
            }

            if (_laguna)
            {
                availableCerberus.Add(_laguna);
            }
        }
        else
        {
            availableCerberus.Add(_cerberusMajor);
        }
    }

    // Replay Level/Proceed Game
    public void UndoLastMove()
    {
        _puzzleContainer.UndoLastMove();
        gameplayEnabled = true;
        // Note: Timer is reset via GameManagerUndoData.Load()
    }

    public void ReplayLevel()
    {
        _puzzleContainer.UndoToFirstMove();
        gameplayEnabled = true;
        // Repopulate availableCerberus
        RepopulateAvailableCerberus();
        // Note: Timer is reset via GameManagerUndoData.Load()
    }

    public void ProceedToNextLevel()
    {
        currentLevel += 1;
        var nextScene = levelSequence.GetSceneBuildIndexForLevel(currentLevel);
        SceneManager.LoadScene(nextScene);
    }
}