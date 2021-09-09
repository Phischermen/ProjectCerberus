﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour, IUndoable
{
    class GameManagerUndoData : UndoData
    {
        private GameManager _gameManager;
        private List<Cerberus> _moveOrder;
        private int _currentMove;
        private int _turn;

        private int _cerberusYetToReachGoal;
        private bool _collectedStar;

        public GameManagerUndoData(GameManager gameManager, List<Cerberus> moveOrder, int currentMove, int turn,
            int cerberusYetToReachGoal, bool collectedStar)
        {
            _gameManager = gameManager;
            _moveOrder = moveOrder;
            _currentMove = currentMove;
            _turn = turn;
            _cerberusYetToReachGoal = cerberusYetToReachGoal;
            _collectedStar = collectedStar;
        }

        public override void Load()
        {
            _gameManager.moveOrder = new List<Cerberus>(_moveOrder);
            _gameManager.currentMove = _currentMove;
            // Start the move of the newly controlled Cerberus
            _gameManager.moveOrder[_currentMove].StartMove();
            _gameManager.turn = _turn;
            _gameManager._cerberusYetToReachGoal = _cerberusYetToReachGoal;
            _gameManager.collectedStar = _collectedStar;
        }
    }

    public string nextScene;
    [HideInInspector] public bool infinteTurns = true;
    [HideInInspector] public int maxTurns;
    [SerializeField] private GameObject _uiPrefab;
    public List<Cerberus> moveOrder { get; protected set; }

    public int turn { get; protected set; }
    public int currentMove { get; protected set; }

    private Laguna _laguna;
    private Jack _jack;
    private Kahuna _kahuna;
    private CerberusMajor _cerberusMajor;

    public bool joinAndSplitEnabled { get; protected set; }

    [HideInInspector] public bool wantsToJoin;
    [HideInInspector] public bool wantsToSplit;
    [HideInInspector] public bool wantsToCycleCharacter;
    [HideInInspector] public bool wantsToUndo;

    private int _cerberusYetToReachGoal;
    [HideInInspector] public bool collectedStar;
    private PuzzleContainer _puzzleContainer;

    void Awake()
    {
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

        // Initialize moveOrder
        moveOrder = new List<Cerberus>();
        if (_jack)
        {
            moveOrder.Add(_jack);
            _cerberusYetToReachGoal += 1;
        }

        if (_kahuna)
        {
            moveOrder.Add(_kahuna);
            _cerberusYetToReachGoal += 1;
        }

        if (_laguna)
        {
            moveOrder.Add(_laguna);
            _cerberusYetToReachGoal += 1;
        }

        // Set initial gameplay variables
        if (_cerberusMajor)
        {
            joinAndSplitEnabled = true;
            _cerberusMajor.SetDisableCollsionAndShowPentagramMarker(true);
        }
    }

    void Update()
    {
        var currentCerberus = moveOrder[currentMove];
        // Process movement of currently controlled cerberus
        var currentCerberusWasOnTopOfGoal = currentCerberus.onTopOfGoal;
        currentCerberus.ProcessMoveInput();
        if (currentCerberus.doneWithMove)
        {
            // Check if they finished the puzzle
            if (currentCerberus.onTopOfGoal && !currentCerberusWasOnTopOfGoal)
            {
                // Decrement goal counter
                _cerberusYetToReachGoal -= (currentCerberus.isCerberusMajor) ? 3 : 1;
                if (_cerberusYetToReachGoal == 0)
                {
                    Debug.Log("You win!");
                    SceneManager.LoadScene(nextScene);
                }
            }
            else if (!currentCerberus.onTopOfGoal && currentCerberusWasOnTopOfGoal)
            {
                // Increment goal counter 
                _cerberusYetToReachGoal += (currentCerberus.isCerberusMajor) ? 3 : 1;
            }


            // Handle request to split/join
            if (wantsToJoin && joinAndSplitEnabled)
            {
                wantsToJoin = false;
                // Check for collision at CerberusMajor
                _cerberusMajor.SetCollisionsEnabled(true);
                _jack.SetCollisionsEnabled(false);
                _kahuna.SetCollisionsEnabled(false);
                _laguna.SetCollisionsEnabled(false);
                if (!_cerberusMajor.CollidesWith(_cerberusMajor.currentCell))
                {
                    FormCerberusMajor();
                    // Don't increment turn if player merges or splits as their first action
                    if (currentMove != 0)
                    {
                        IncrementTurn();
                    }
                }
                else
                {
                    _cerberusMajor.SetCollisionsEnabled(false);
                    _jack.SetCollisionsEnabled(true);
                    _kahuna.SetCollisionsEnabled(true);
                    _laguna.SetCollisionsEnabled(true);
                }
            }
            else if (wantsToSplit && joinAndSplitEnabled)
            {
                wantsToSplit = false;
                // Check for collision with every dog
                _cerberusMajor.SetCollisionsEnabled(false);
                _jack.SetCollisionsEnabled(true);
                _kahuna.SetCollisionsEnabled(true);
                _laguna.SetCollisionsEnabled(true);
                if (!_jack.CollidesWith(_jack.currentCell) &&
                    !_laguna.CollidesWith(_laguna.currentCell) &&
                    !_kahuna.CollidesWith(_kahuna.currentCell))
                {
                    SplitCerberusMajor();
                    // Don't increment turn if player merges or splits as their first action
                    if (currentMove != 0)
                    {
                        IncrementTurn();
                    }
                }
                else
                {
                    _cerberusMajor.SetCollisionsEnabled(true);
                    _jack.SetCollisionsEnabled(false);
                    _kahuna.SetCollisionsEnabled(false);
                    _laguna.SetCollisionsEnabled(false);
                }
            }
            // Increment turn normally
            else
            {
                currentMove += 1;
                if (currentMove >= moveOrder.Count)
                {
                    // All cerberus have moved. Start next turn
                    IncrementTurn();
                }
            }

            // Start next cerberus's move
            var nextCerberus = moveOrder[currentMove];
            nextCerberus.StartMove();
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
            ChangeCerberusSpot(currentCerberus, moveOrder.Count - 1);
            // Initialize the new cerberus's move
            var newCerberus = moveOrder[currentMove];
            newCerberus.StartMove();
        }
    }

    // Undo

    public UndoData GetUndoData()
    {
        var moveOrderCopy = new List<Cerberus>(moveOrder);
        var undoData = new GameManagerUndoData(this, moveOrderCopy, currentMove, turn, _cerberusYetToReachGoal,
            collectedStar);
        return undoData;
    }

    // Move order management
    void ChangeCerberusSpot(Cerberus cerberus, int newSpot)
    {
        moveOrder.Remove(cerberus);
        moveOrder.Insert(newSpot, cerberus);
    }

    // Turn management
    void IncrementTurn()
    {
        turn += 1;
        if (!infinteTurns && turn > maxTurns)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        currentMove = 0;
        Debug.Log(turn);
    }

    void GoBackToTurn(int newTurn)
    {
    }

    // Merge and split Management
    public void FormCerberusMajor()
    {
        // Stop animations
        _jack.FinishCurrentAnimation();
        _kahuna.FinishCurrentAnimation();
        _laguna.FinishCurrentAnimation();

        _cerberusMajor.SetDisableCollsionAndShowPentagramMarker(false);
        _jack.SetDisableCollsionAndShowPentagramMarker(true);
        _kahuna.SetDisableCollsionAndShowPentagramMarker(true);
        _laguna.SetDisableCollsionAndShowPentagramMarker(true);

        moveOrder.Clear();
        moveOrder.Add(_cerberusMajor);
    }

    public void SplitCerberusMajor()
    {
        // Stop animation
        _cerberusMajor.FinishCurrentAnimation();

        _cerberusMajor.SetDisableCollsionAndShowPentagramMarker(true);
        _jack.SetDisableCollsionAndShowPentagramMarker(false);
        _kahuna.SetDisableCollsionAndShowPentagramMarker(false);
        _laguna.SetDisableCollsionAndShowPentagramMarker(false);

        moveOrder.Clear();
        moveOrder.Add(_jack);
        moveOrder.Add(_kahuna);
        moveOrder.Add(_laguna);
    }
}