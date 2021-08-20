using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject _uiPrefab;
    public List<Cerberus> moveOrder { get; protected set; }
    public int turn { get; protected set; }
    public int currentMove { get; protected set; }

    private Laguna _laguna;
    private Jack _jack;
    private Kahuna _kahuna;
    private CerberusMajor _cerberusMajor;

    private CerberusMajorSpawnPoint _cerberusMajorSpawnPoint;

    public bool joinAndSplitEnabled { get; protected set; }
    [HideInInspector] public bool wantsToJoin;
    [HideInInspector] public bool wantsToSplit;
    [HideInInspector] public bool wantsToCycleCharacter;

    private int _cerberusYetToReachGoal;

    void Awake()
    {
        Instantiate(_uiPrefab);
    }

    void Start()
    {
        // Get objects
        _jack = FindObjectOfType<Jack>();
        _kahuna = FindObjectOfType<Kahuna>();
        _laguna = FindObjectOfType<Laguna>();
        _cerberusMajor = FindObjectOfType<CerberusMajor>();
        _cerberusMajorSpawnPoint = FindObjectOfType<CerberusMajorSpawnPoint>();

        // Initialize moveOrder
        moveOrder = new List<Cerberus>();
        if (_jack) moveOrder.Add(_jack);
        if (_kahuna) moveOrder.Add(_kahuna);
        if (_laguna) moveOrder.Add(_laguna);

        // Set initial gameplay variables
        if (_cerberusMajor)
        {
            joinAndSplitEnabled = true;
            _cerberusMajor.SetDisableCollsionAndShowPentagramMarker(true);
        }

        _cerberusYetToReachGoal = FindObjectsOfType<Finish>().Length;
    }

    // Update is called once per frame
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
                _cerberusYetToReachGoal -= 1;
                if (_cerberusYetToReachGoal == 0)
                {
                    Debug.Log("You win!");
                }
            }
            else
            {
                // Increment goal counter 
                _cerberusYetToReachGoal += 1;
            }


            // Handle request to split/join
            if (wantsToJoin && joinAndSplitEnabled)
            {
                wantsToJoin = false;
                FormCerberusMajor();
                IncrementTurn();
            }
            else if (wantsToSplit && joinAndSplitEnabled)
            {
                wantsToSplit = false;
                SplitCerberusMajor();
                IncrementTurn();
            }
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
        // Handle request to cycle character
        else if (wantsToCycleCharacter)
        {
            wantsToCycleCharacter = false;
            ChangeCerberusSpot(currentCerberus, moveOrder.Count - 1);
        }
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
        currentMove = 0;
        Debug.Log(turn);
    }

    void GoBackToTurn(int newTurn)
    {
    }

    // Merge and split Management
    public void FormCerberusMajor()
    {
        _cerberusMajor.SetDisableCollsionAndShowPentagramMarker(false);
        _jack.SetDisableCollsionAndShowPentagramMarker(true);
        _kahuna.SetDisableCollsionAndShowPentagramMarker(true);
        _laguna.SetDisableCollsionAndShowPentagramMarker(true);

        moveOrder.Clear();
        moveOrder.Add(_cerberusMajor);
    }

    public void SplitCerberusMajor()
    {
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