/*
 * Game manager searches the scene on Start() for critical components, such as JKL, and manages their split, join, undo,
 * and cycle abilities. Game manager also defines certain constraints for the level such as time limit and max moves
 * until star loss. Game manager is also responsible for transitioning from gameplay to win/lose states, and loading the
 * next scene.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Photon.Pun;
using Photon.Realtime;
using Priority_Queue;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviourPunCallbacks, IUndoable
{
    class GameManagerStateData : StateData
    {
        private GameManager _gameManager;
        private Cerberus _currentCerberus;

        // These values are networked via an RPC.
        private int _move;
        private float _timer;

        public bool joinSplitEnabled => booleans[0];
        public bool cerberusFormed => booleans[1];

        public bool collectedStar => booleans[2];

        public GameManagerStateData(GameManager gameManager, int move, float timer, Cerberus currentCerberus,
            bool cerberusFormed, bool joinSplitEnabled, bool collectedStar)
        {
            _gameManager = gameManager;
            _currentCerberus = currentCerberus;
            _move = move;
            _timer = timer;
            booleans[0] = joinSplitEnabled;
            booleans[1] = cerberusFormed;
            booleans[2] = collectedStar;
        }

        public override void Load()
        {
            // Start the move of the newly controlled Cerberus
            _gameManager.currentCerberus = _currentCerberus;
            _currentCerberus.StartMove();
            _gameManager.move = _move;
            _gameManager.timer = _timer;

            // Stop timer if first move. Otherwise run timer.
            _gameManager._timerRunning = _move != 0;

            _gameManager.cerberusFormed = cerberusFormed;
            _gameManager.joinAndSplitEnabled = joinSplitEnabled;

            _gameManager.collectedStar = collectedStar;
            // Repopulate available cerberus
            _gameManager.RepopulateAvailableCerberus(!cerberusFormed);
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
    [SerializeField] private GameObject _pauseMenu;

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

    [HideInInspector] public bool playMusicAtStart;

    public Queue<Cerberus.CerberusCommand> _commandQueue;


    void Awake()
    {
        // Create UI
        Instantiate(_uiPrefab);
        // Load Level Sequence and get current world and level
        if (levelSequence != MainMenuController.chosenLevelSequence)
        {
            levelSequence = MainMenuController.chosenLevelSequence;
            currentLevel = levelSequence.FindCurrentLevelSequence(SceneManager.GetActiveScene().buildIndex);
            playMusicAtStart = true;
        }
    }

    void Start()
    {
        if (playMusicAtStart)
        {
            // Play music in case user plays a level at the beginning of the game.
            levelSequence.GetSceneBuildIndexForLevel(currentLevel, andPlayMusic: true);
        }

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
        cerberusToUserMap = new[] {-1, -1, -1};
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

        if (_cerberusMajor && _cerberusThatMustReachGoal == 0)
        {
            // Cerberus Major is the only cerberus prefab in the scene.
            availableCerberus.Add(_cerberusMajor);
            _cerberusThatMustReachGoal = 3;
            cerberusFormed = true;
        }

        if (!PhotonNetwork.InRoom)
        {
            currentCerberus = availableCerberus[0];
        }
        else
        {
            // All Multiplayer rooms should have all three dogs, so this should be safe.
            currentCerberus = availableCerberus[MainMenuController.defaultDog];
            // Sync this setting.
            SendRPCCycleCharacter(-1, MainMenuController.defaultDog);
        }

        // Setup camera.
        _cameraController.SetCameraMode(startInFixedCameraMode
            ? PuzzleCameraController.CameraMode.FixedPointMode
            : PuzzleCameraController.CameraMode.ScrollingMode);
        _cameraController.GotoDesiredPositionAndSize();

        // Set initial gameplay variables
        if (_cerberusMajor && (_jack || _kahuna || _laguna))
        {
            // Player may only merge or split if cerberus major is in scene and at least one of dog's is in the scene.
            joinAndSplitEnabled = true;
            // Cerberus Major is inactive by default
            _cerberusMajor.SetDisableCollsionAndShowPentagramMarker(true);
        }

        timer = 0;
        _timerRunning = false;
        gameplayEnabled = true;

        // Initialize Command Queue.
        _commandQueue = new Queue<Cerberus.CerberusCommand>();
    }

    void Update()
    {
        if (_input.pause && gameplayEnabled)
        {
            gameplayEnabled = false;
            Instantiate(_pauseMenu);
        }

        // Process movement of currently controlled cerberus if gameplay is enabled.
        if (gameplayEnabled)
        {
            var nextMoveNeedsToStart = false;
            currentCerberus.CheckInputForResetUndoOrCycle();
            var command = currentCerberus.ProcessInputIntoCommand();
            // If commanded to do something, send command to master client.
            if (command.doSomething)
            {
                SendRPCEnqueueCommand(command);
            }

            // If there are commands, get the first one.
            var cerberusDoneWithMove = false;
            if (_commandQueue.Count > 0)
            {
                var nextCommand = _commandQueue.Dequeue();

                if (nextCommand.cerberusId == 0 && _jack != null)
                {
                    _jack.InterpretCommand(nextCommand);
                    cerberusDoneWithMove = _jack.doneWithMove;
                }
                else if (nextCommand.cerberusId == 1 && _kahuna != null)
                {
                    _kahuna.InterpretCommand(nextCommand);
                    cerberusDoneWithMove = _kahuna.doneWithMove;
                }
                else if (nextCommand.cerberusId == 2 && _laguna != null)
                {
                    _laguna.InterpretCommand(nextCommand);
                    cerberusDoneWithMove = _laguna.doneWithMove;
                }
                else if (nextCommand.cerberusId == 3 && _cerberusMajor != null)
                {
                    _cerberusMajor.InterpretCommand(nextCommand);
                    cerberusDoneWithMove = _cerberusMajor.doneWithMove;
                }
            }


            // Check if cerberus made their move
            if (cerberusDoneWithMove)
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

            // Handle request to undo. Must be master client if connected
            if (wantsToUndo && (!PhotonNetwork.InRoom ||
                                PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient))
            {
                wantsToUndo = false;
                _puzzleContainer.UndoLastMove();
                SendRPCUndoLastMove();
            }

            // Handle request to cycle character
            if (wantsToCycleCharacter && !cerberusFormed && PhotonNetwork.PlayerList.Length != 3)
            {
                wantsToCycleCharacter = false;
                // Get index of currentCerberus before sorting.
                var oldIdx = availableCerberus.IndexOf(currentCerberus);
                // Cull the one possessed by the other client, then sort availableCerberus, from north-west most to south-east most.
                var availableCerberusSorted = availableCerberus
                    .Where((cerberus, i) => cerberusToUserMap[i] == -1 || cerberus == currentCerberus)
                    .OrderBy(CompareCerberusByPosition).ToList();
                // Get index of currentCerberus after sorting the list.
                var currentIdxInSorted = availableCerberusSorted.IndexOf(currentCerberus);
                if (_input.cycleCharacterForward)
                {
                    // Switch to Cerberus south-east of current Cerberus.
                    var idx = (currentIdxInSorted + 1) % availableCerberusSorted.Count;
                    currentCerberus = availableCerberusSorted[idx];
                }
                else if (_input.cycleCharacterBackward)
                {
                    // Switch to Cerberus north-west of current Cerberus.
                    // Note: To avoid getting a negative index from % operator, I add the array length to currentIndex. 
                    var idx = ((currentIdxInSorted - 1 + availableCerberusSorted.Count) %
                               availableCerberusSorted.Count);
                    currentCerberus = availableCerberusSorted[idx];
                }
                // TODO: Check if those specific characters are controlled.
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

                var newIdx = availableCerberus.IndexOf(currentCerberus);
                SendRPCCycleCharacter(oldIdx, newIdx);
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
            if (PhotonNetwork.IsMasterClient && Time.frameCount % 960 == 0)
            {
                var objectArray = _puzzleContainer.GetStateDataFromUndoables();
                SendRPCSyncBoard(objectArray);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawIcon(new Vector3(0, 3, 0), "Gears");
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
    public StateData GetUndoData()
    {
        var undoData = new GameManagerStateData(this, move, timer, currentCerberus, cerberusFormed, joinAndSplitEnabled,
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
        // Todo verify clicked cerberus, or just switch to previously controlled character.
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
        // Note: Timer is reset via GameManagerStateData.Load()
    }

    public void ReplayLevel()
    {
        _puzzleContainer.UndoToFirstMove();
        // NOTE: Only master client can send the following command.
        SendRPCReplayLevel();
        gameplayEnabled = true;
        // Repopulate availableCerberus
        RepopulateAvailableCerberus();
        // Note: Timer is reset via GameManagerStateData.Load()
    }

    public void ProceedToNextLevel()
    {
        var nextScene = levelSequence.GetSceneBuildIndexForLevel(currentLevel + 1, andPlayMusic: true);
        if (nextScene == -1)
        {
            Debug.Log($"Could not find next level {currentLevel + 1}");
        }
        else
        {
            currentLevel += 1;
            SceneManager.LoadScene(nextScene);
        }
    }

    // Multiplayer Callbacks + Methods
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene((int) Scenum.Scene.MainMenu);
    }

    public bool LeaveRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            Debug.Log("Room left.");
            return true;
        }

        return false;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", newPlayer.NickName); // not seen if you're the player connecting


        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}",
                PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
            SendRPCSyncBoard(_puzzleContainer.GetStateDataFromUndoables());
        }
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects


        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}",
                PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
        }
    }

    public AudioClip testAudio;

    public void SendRPCEnqueueCommand(Cerberus.CerberusCommand command)
    {
        if (PhotonNetwork.InRoom)
        {
            photonView.RPC(nameof(RPCEnqueueCommand), RpcTarget.All, command);
        }
        else
        {
            RPCEnqueueCommand(command);
        }
    }

    [PunRPC]
    public void RPCEnqueueCommand(Cerberus.CerberusCommand command)
    {
        _commandQueue.Enqueue(command);
    }

    public void SendRPCSyncBoard(StateData[] objectArray)
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(RPCSyncBoard), RpcTarget.Others, objectArray, timer, move, currentLevel,
                cerberusToUserMap);
        }
        // No reason to sync board if there's only one client.
    }

    [PunRPC]
    public void RPCSyncBoard(StateData[] stateDatas, float pTimer, int pMove, int pCurrentLevel, int[] map)
    {
        _puzzleContainer.SyncBoardWithData(stateDatas);
        timer = pTimer;
        move = pMove;
        currentLevel = pCurrentLevel;
        cerberusToUserMap = map;
        // Ensure correct cerberus is possessed.
        currentCerberus = cerberusFormed
            ? _cerberusMajor
            : availableCerberus[cerberusToUserMap.ToList().IndexOf(PhotonNetwork.LocalPlayer.ActorNumber)];
    }

    public void SendRPCReplayLevel()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(RPCReplayLevel), RpcTarget.Others);
        }
    }

    [PunRPC]
    public void RPCReplayLevel()
    {
        ReplayLevel();
        DestroyEndcardUI();
    }

    public void SendRPCUndoLastMove()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(RPCUndoLastMove), RpcTarget.Others);
        }
    }

    [PunRPC]
    public void RPCUndoLastMove()
    {
        UndoLastMove();
        DestroyEndcardUI();
    }

    public void DestroyEndcardUI()
    {
        // Destroy endcard UI that may or may not be present.
        var puzzleUIEndCardSuccess = FindObjectOfType<PuzzleUIEndCardSuccess>();
        if (puzzleUIEndCardSuccess != null)
        {
            Destroy(puzzleUIEndCardSuccess);
        }

        var puzzleUIEndCardFailure = FindObjectOfType<PuzzleUIEndCardFailure>();
        if (puzzleUIEndCardFailure != null)
        {
            Destroy(puzzleUIEndCardFailure);
        }
    }

    public void SendRPCCycleCharacter(int oldDog, int newDog)
    {
        if (oldDog == newDog) return;
        if (PhotonNetwork.InRoom)
        {
            photonView.RPC(nameof(RPCCycleCharacter), RpcTarget.All, oldDog, newDog,
                PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    [PunRPC]
    public void RPCCycleCharacter(int oldDog, int newDog, int actorId)
    {
        // Swap old and new.
        if (oldDog != -1)
        {
            cerberusToUserMap[oldDog] = cerberusToUserMap[newDog];
        }

        cerberusToUserMap[newDog] = actorId;
    }
}