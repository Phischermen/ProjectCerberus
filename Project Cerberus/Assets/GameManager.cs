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
        if (_cerberusMajor && _cerberusMajorSpawnPoint) joinAndSplitEnabled = true;
        _cerberusYetToReachGoal = FindObjectsOfType<Finish>().Length;
        if (_cerberusMajor) _cerberusMajor.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        var currentCerberus = moveOrder[currentMove];
        // Process movement of currently controlled cerberus
        currentCerberus.ProcessMoveInput();
        if (currentCerberus.doneWithMove)
        {
            // Check if they finished the puzzle
            if (currentCerberus.finishedPuzzle)
            {
                // Cerberus has finished puzzle. Disable control of Cerberus
                currentCerberus.gameObject.SetActive(false);
                moveOrder.Remove(currentCerberus);
                joinAndSplitEnabled = false;
                _cerberusYetToReachGoal -= 1;
                if (_cerberusYetToReachGoal == 0)
                {
                    Debug.Log("You win!");
                    gameObject.SetActive(false);
                }
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
    }

    // Move order management
    void ChangeCerberusSpot(Cerberus cerberus, int newSpot)
    {
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
        _cerberusMajor.gameObject.SetActive(true);
        _cerberusMajor.Move(_cerberusMajorSpawnPoint.position);
        _jack.gameObject.SetActive(false);
        _kahuna.gameObject.SetActive(false);
        _laguna.gameObject.SetActive(false);

        moveOrder.Clear();
        moveOrder.Add(_cerberusMajor);
    }

    public void SplitCerberusMajor()
    {
        _cerberusMajor.gameObject.SetActive(false);
        moveOrder.Clear();
        ReenableCerberusIfYetToFinishPuzzle(_jack);
        ReenableCerberusIfYetToFinishPuzzle(_kahuna);
        ReenableCerberusIfYetToFinishPuzzle(_laguna);
    }

    private void ReenableCerberusIfYetToFinishPuzzle(Cerberus cerberus)
    {
        if (!cerberus.finishedPuzzle)
        {
            cerberus.gameObject.SetActive(true);
            moveOrder.Add(cerberus);
        }
    }
}