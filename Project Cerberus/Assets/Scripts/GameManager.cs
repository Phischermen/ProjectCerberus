using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
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
        private bool _cerberusFormed;
        private int _cerberusYetToReachGoal;
        private bool _collectedStar;
        
        public GameManagerUndoData(GameManager gameManager, int move, Cerberus currentCerberus,
            int cerberusYetToReachGoal, bool cerberusFormed, bool collectedStar)
        {
            _gameManager = gameManager;
            _currentCerberus = currentCerberus;
            _move = move;
            _cerberusYetToReachGoal = cerberusYetToReachGoal;
            _cerberusFormed = cerberusFormed;
            _collectedStar = collectedStar;
        }

        public override void Load()
        {
            // Start the move of the newly controlled Cerberus
            _currentCerberus.StartMove();
            _gameManager._cerberusYetToReachGoal = _cerberusYetToReachGoal;
            _gameManager.cerberusFormed = _cerberusFormed;
            _gameManager.collectedStar = _collectedStar;
        }
    }

    public string nextScene;
    
    [HideInInspector] public bool infinteMovesTilStarLoss = true;
    [HideInInspector] public int maxMovesUntilStarLoss;

    [SerializeField] private GameObject _uiPrefab;
    
    
    public int move { get; protected set; }

    private Laguna _laguna;
    private Jack _jack;
    private Kahuna _kahuna;
    private CerberusMajor _cerberusMajor;
    public List<Cerberus> availableCerberus { get; protected set; }
    [HideInInspector] public Cerberus currentCerberus;
    private PuzzleContainer _puzzleContainer;

    public bool joinAndSplitEnabled { get; protected set; }
    public bool cerberusFormed { get; protected set; }

    [HideInInspector] public bool wantsToJoin;
    [HideInInspector] public bool wantsToSplit;
    [HideInInspector] public bool wantsToCycleCharacter;
    [HideInInspector] public bool wantsToUndo;

    private int _cerberusYetToReachGoal;
    [HideInInspector] public bool collectedStar;

    void Awake()
    {
        // Create UI
        Instantiate(_uiPrefab);
    }

    void Start()
    {
        // Get objects
        _puzzleContainer = FindObjectOfType<PuzzleContainer>();

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
    }

    void Update()
    {
        // Cache onTopOfGoal, to see if Cerberus enters/exits goal.
        var currentCerberusWasOnTopOfGoal = currentCerberus.onTopOfGoal;
        // Process movement of currently controlled cerberus
        currentCerberus.ProcessMoveInput();
        // Check if cerberus made their move
        if (currentCerberus.doneWithMove)
        {
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
                    // Player wins!
                    Debug.Log("You win!");
                    // Goto next scene
                    SceneManager.LoadScene(nextScene);
                }
            }

            if (joinAndSplitEnabled)
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
            }

            // Increment move
            move += 1;
            // Start the next move with currentCerberus
            currentCerberus.StartMove();
        }
        // Handle request to undo
        else if (wantsToUndo)
        {
            wantsToUndo = false;
            _puzzleContainer.UndoLastMove();
            Debug.Log("Ündo");
        }
        // Handle request to cycle character
        else if (wantsToCycleCharacter)
        {
            wantsToCycleCharacter = false;
            // TODO Implement method to get next cerberus to control based on position of currentCerberus.
            currentCerberus =
                availableCerberus[(availableCerberus.FindIndex(cerberus => cerberus == currentCerberus) + 1) % availableCerberus.Count];
            currentCerberus.StartMove();
        }
    }

    // Undo

    public UndoData GetUndoData()
    {
        var undoData = new GameManagerUndoData(this, move, currentCerberus, _cerberusYetToReachGoal, cerberusFormed,
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
}