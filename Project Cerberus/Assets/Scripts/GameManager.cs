/*
 * Game manager searches the scene on Start() for critical components, such as JKL, and manages their split, join, undo,
 * and cycle abilities. Game manager also defines certain constraints for the level such as time limit and max moves
 * until star loss. Game manager is also responsible for transitioning from gameplay to win/lose states
 * (NOT YET IMPLEMENTED), and loading the next scene
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour, IUndoable
{
    class GameManagerUndoData : UndoData
    {
        private GameManager _gameManager;
        private Cerberus _currentCerberus;

        private int _move;
        private float _timer;
        private bool _cerberusFormed;
        private int _cerberusYetToReachGoal;
        private bool _collectedStar;

        public GameManagerUndoData(GameManager gameManager, int move, float timer, Cerberus currentCerberus,
            int cerberusYetToReachGoal, bool cerberusFormed, bool collectedStar)
        {
            _gameManager = gameManager;
            _currentCerberus = currentCerberus;
            _move = move;
            _timer = timer;
            _cerberusYetToReachGoal = cerberusYetToReachGoal;
            _cerberusFormed = cerberusFormed;
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

            _gameManager._cerberusYetToReachGoal = _cerberusYetToReachGoal;
            _gameManager.cerberusFormed = _cerberusFormed;
            _gameManager.collectedStar = _collectedStar;
        }
    }

    private static LevelSequence _levelSequence;
    private static int currentWorld = -1;
    private static int currentLevel = -1;

    [HideInInspector] public bool infiniteTimeLimit = true;
    [HideInInspector] public float timeLimit = 1f;
    [HideInInspector] public float parTime = 1f;

    private bool _timerRunning = false;

    // TODO Make timer run forwards instead of backwards so time can be displayed even in a level with an infinite time limit
    public float timer { get; protected set; }

    public int move { get; protected set; }
    [HideInInspector] public bool infinteMovesTilStarLoss = true;
    [HideInInspector] public int maxMovesUntilStarLoss;

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

    public bool joinAndSplitEnabled { get; protected set; }
    public bool cerberusFormed { get; protected set; }

    [HideInInspector] public bool wantsToJoin;
    [HideInInspector] public bool wantsToSplit;
    [HideInInspector] public bool wantsToCycleCharacter;
    [HideInInspector] public bool wantsToUndo;

    private int _cerberusYetToReachGoal;
    [HideInInspector] public bool collectedStar;

    [HideInInspector] public bool gameplayEnabled;

    void Awake()
    {
        // Create UI
        Instantiate(_uiPrefab);
        // Load Level Sequence and get current world and level
        if (_levelSequence == null)
        {
            _levelSequence = Resources.Load<CustomProjectSettings>(CustomProjectSettings.resourcePath)
                .mainLevelSequence;
            _levelSequence.FindCurrentLevelAndWorld(SceneManager.GetActiveScene().buildIndex, out currentLevel,
                out currentWorld);
        }
    }

    void Start()
    {
        // Get objects
        _puzzleContainer = FindObjectOfType<PuzzleContainer>();
        _input = FindObjectOfType<PuzzleGameplayInput>();

        _jack = FindObjectOfType<Jack>();
        _kahuna = FindObjectOfType<Kahuna>();
        _laguna = FindObjectOfType<Laguna>();
        _cerberusMajor = FindObjectOfType<CerberusMajor>();

        // Initialize availableCerberus
        availableCerberus = new List<Cerberus>();
        if (_jack)
        {
            availableCerberus.Add(_jack);
            _cerberusYetToReachGoal += 1;
        }

        if (_kahuna)
        {
            availableCerberus.Add(_kahuna);
            _cerberusYetToReachGoal += 1;
        }

        if (_laguna)
        {
            availableCerberus.Add(_laguna);
            _cerberusYetToReachGoal += 1;
        }

        currentCerberus = availableCerberus[0];

        // Set initial gameplay variables
        if (_cerberusMajor)
        {
            // Player may only merge or split if cerberus major is in scene
            joinAndSplitEnabled = true;
            // Cerberus Major is inactive by default
            _cerberusMajor.SetDisableCollsionAndShowPentagramMarker(true);
        }

        // Sort availableCerberus for PuzzleUI.
        availableCerberus.Sort(CompareCerberusByPosition);

        timer = 0;
        _timerRunning = false;
        gameplayEnabled = true;
    }

    void Update()
    {
        // Cache onTopOfGoal, to see if Cerberus enters/exits goal.
        var currentCerberusWasOnTopOfGoal = currentCerberus.onTopOfGoal;

        // Process movement of currently controlled cerberus if there is still time left and gameplay is enabled.
        if (gameplayEnabled && (timer < timeLimit || infiniteTimeLimit))
        {
            currentCerberus.ProcessMoveInput();
            // Check if cerberus made their move
            if (currentCerberus.doneWithMove)
            {
                // Start timer
                _timerRunning = true;
                // Check if currentCerberus entered/exited a goal
                if (!currentCerberus.onTopOfGoal || currentCerberusWasOnTopOfGoal)
                {
                    if (!currentCerberus.onTopOfGoal && currentCerberusWasOnTopOfGoal)
                    {
                        // Increment _cerberusYetToReachGoal. 
                        _cerberusYetToReachGoal += (currentCerberus.isCerberusMajor) ? 3 : 1;
                    }
                }
                else
                {
                    // Decrement _cerberusYetToReachGoal. Cerberus Major represents JKL combined, hence why we decrement by
                    // three if Cerberus Major steps off goal.
                    _cerberusYetToReachGoal -= (currentCerberus.isCerberusMajor) ? 3 : 1;
                    if (_cerberusYetToReachGoal == 0)
                    {
                        // Victory! Winning!
                        // Play victory animation
                        EndGameWithSuccessStatus();
                    }
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
                        // Set collisionsEnabled in preparation for collision test. JKL collisions are disabled in case they
                        // happen to be standing on top of Cerberus.
                        _cerberusMajor.SetCollisionsEnabled(true);
                        _jack.SetCollisionsEnabled(false);
                        _kahuna.SetCollisionsEnabled(false);
                        _laguna.SetCollisionsEnabled(false);
                        // Check for collision at CerberusMajor
                        if (_cerberusMajor.CollidesWith(_cerberusMajor.currentCell))
                        {
                            // Reset collisionsEnabled
                            _cerberusMajor.SetCollisionsEnabled(false);
                            _jack.SetCollisionsEnabled(true);
                            _kahuna.SetCollisionsEnabled(true);
                            _laguna.SetCollisionsEnabled(true);
                        }
                        else
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
                        _cerberusMajor.SetCollisionsEnabled(false);
                        _jack.SetCollisionsEnabled(true);
                        _kahuna.SetCollisionsEnabled(true);
                        _laguna.SetCollisionsEnabled(true);
                        if (_jack.CollidesWith(_jack.currentCell) || _kahuna.CollidesWith(_kahuna.currentCell) ||
                            _laguna.CollidesWith(_laguna.currentCell))
                        {
                            // Reset collisionEnabled
                            _cerberusMajor.SetCollisionsEnabled(true);
                            _jack.SetCollisionsEnabled(false);
                            _kahuna.SetCollisionsEnabled(false);
                            _laguna.SetCollisionsEnabled(false);
                        }
                        else
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

                // Sort availableCerberus, from north-west most to south-east most.
                availableCerberus.Sort(CompareCerberusByPosition);
                // Command PuzzleContainer to process entities in response to this move
                _puzzleContainer.ProcessEntitiesInResponseToPlayerMove();
                // Start the next move with currentCerberus
                currentCerberus.StartMove();
            }
            // Handle request to undo
            else if (wantsToUndo)
            {
                wantsToUndo = false;
                _puzzleContainer.UndoLastMove();
            }
            // Handle request to cycle character
            else if (wantsToCycleCharacter && !cerberusFormed)
            {
                wantsToCycleCharacter = false;
                // Get index of currentCerberus after sorting the list.
                var currentIdx = availableCerberus.IndexOf(currentCerberus);
                if (_input.cycleCharacterForward)
                {
                    // Switch to Cerberus south-east of current Cerberus.
                    var idx = (currentIdx + 1) % availableCerberus.Count;
                    currentCerberus = availableCerberus[idx];
                }
                else if (_input.cycleCharacterBackward)
                {
                    // Switch to Cerberus north-west of current Cerberus.
                    // Note: To avoid getting a negative index from % operator, I add the array length to currentIndex. 
                    var idx = ((currentIdx - 1 + availableCerberus.Count) % availableCerberus.Count);
                    currentCerberus = availableCerberus[idx];
                }
                else if (_input.cycleCharacter0)
                {
                    // Switch to Cerberus at far north-west.
                    currentCerberus = availableCerberus[0];
                }
                else if (_input.cycleCharacter1)
                {
                    // Switch to the Cerberus between Cerberus.
                    currentCerberus = availableCerberus[1];
                }
                else if (_input.cycleCharacter2)
                {
                    // Switch to Cerberus at far south-east.
                    currentCerberus = availableCerberus[2];
                }

                currentCerberus.StartMove();
            }
        }

        // Run timer
        if (_timerRunning)
        {
            timer += Time.deltaTime;
            if (timer > timeLimit && !infiniteTimeLimit)
            {
                // Time's up! Game over!
                EndGameWithFailureStatus();
            }
        }
    }

    // Gameover
    public void EndGameWithFailureStatus()
    {
        // Disable gameplay
        gameplayEnabled = false;
        // Stop timer
        _timerRunning = false;
        timer = timeLimit;
        // Display game over end card.
        Instantiate(_gameOverEndCard);
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
    private int CompareCerberusByPosition(Cerberus a, Cerberus b)
    {
        return (a.position.x * PuzzleContainer.maxLevelWidth + (PuzzleContainer.maxLevelWidth - a.position.y)) -
               (b.position.x * PuzzleContainer.maxLevelWidth + (PuzzleContainer.maxLevelWidth - b.position.y));
    }

    // Undo
    public UndoData GetUndoData()
    {
        var undoData = new GameManagerUndoData(this, move, timer, currentCerberus, _cerberusYetToReachGoal,
            cerberusFormed,
            collectedStar);
        return undoData;
    }

    // Merge and split Management
    public void FormCerberusMajor()
    {
        cerberusFormed = true;
        // Stop animations for JKL
        _jack.FinishCurrentAnimation();
        _kahuna.FinishCurrentAnimation();
        _laguna.FinishCurrentAnimation();
        // Activate Cerberus Major and deactivate JKL
        _cerberusMajor.SetDisableCollsionAndShowPentagramMarker(false);
        _jack.SetDisableCollsionAndShowPentagramMarker(true);
        _kahuna.SetDisableCollsionAndShowPentagramMarker(true);
        _laguna.SetDisableCollsionAndShowPentagramMarker(true);

        currentCerberus = _cerberusMajor;
    }

    public void SplitCerberusMajor()
    {
        cerberusFormed = false;
        // Stop animation for Cerberus Major
        _cerberusMajor.FinishCurrentAnimation();
        // Activate JKL and deactivate Cerberus Major
        _cerberusMajor.SetDisableCollsionAndShowPentagramMarker(true);
        _jack.SetDisableCollsionAndShowPentagramMarker(false);
        _kahuna.SetDisableCollsionAndShowPentagramMarker(false);
        _laguna.SetDisableCollsionAndShowPentagramMarker(false);

        currentCerberus = _jack;
    }

    // Replay Level/Proceed Game
    public void ReplayLevel()
    {
        _puzzleContainer.UndoToFirstMove();
        gameplayEnabled = true;
        // Note: Timer is reset via GameManagerUndoData.Load()
    }

    public void ProceedToNextLevel()
    {
        // Goto next scene
        var nextLevel = currentLevel + 1;
        var nextWorld = currentWorld;
        if (nextLevel >= _levelSequence.GetNumberOfLevelsInWorld(currentWorld))
        {
            nextWorld += 1;
            if (nextWorld >= _levelSequence.GetNumberOfWorlds())
            {
                nextWorld = 0;
            }

            nextLevel = 0;
        }

        var nextScene = _levelSequence.GetLevel(nextWorld, nextLevel);
        currentLevel = nextLevel;
        currentWorld = nextWorld;
        SceneManager.LoadScene(nextScene);
    }
}