using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DialoguePanel : MonoBehaviourPun
{
    public static DialoguePanel i;
    public static int charPerSecond = 37;
    private static int dialoguePanelViewId = 2001;

    [FormerlySerializedAs("text")] public Text textDisplay;
    public CanvasGroup canvasGroup;

    public bool typing;
    public bool displayingMessage;
    private WaitForSeconds _waitForSeconds;
    private WaitUntil _waitUntilDismissed;
    private WaitUntil _waitUntilDismissedOrTimeUp;
    public int networkedDismissalRequests;

    private float timeLastCharPrinted;

    private PuzzleGameplayInput _input;
    private GameManager _gameManager;

    // Start is called before the first frame update
    void Start()
    {
        i = this;
        photonView.ViewID = dialoguePanelViewId;
        _input = FindObjectOfType<PuzzleGameplayInput>();
        _gameManager = FindObjectOfType<GameManager>();
        _waitForSeconds = new WaitForSeconds(1f / charPerSecond);
        _waitUntilDismissed = new WaitUntil(IsDismissalRequested);
        _waitUntilDismissedOrTimeUp =
            new WaitUntil(() => IsDismissalRequested() || (Time.time - timeLastCharPrinted) > (1f / charPerSecond));
        canvasGroup.alpha = 0;
    }

    public void StartConversation()
    {
        _gameManager.gameplayEnabled = false;
    }

    public void EndConversation()
    {
        _gameManager.gameplayEnabled = true;
    }

    public IEnumerator DisplayDialogue(Vector2Int dialogueKey)
    {
        var message = CustomProjectSettings.i.dialogueDatabaseAsset.scenes[dialogueKey.x].dialogues[dialogueKey.y].line;
        textDisplay.text = "";
        displayingMessage = true;
        typing = true;
        canvasGroup.alpha = 1;
        foreach (char letter in message)
        {
            textDisplay.text += letter;
            timeLastCharPrinted = Time.time;
            yield return _waitUntilDismissedOrTimeUp;
            // Display full message if player is obviously trying to skip.
            if (IsDismissalRequested())
            {
                InterpretAndHandleDismissal();
                break;
            }
        }

        typing = false;
        textDisplay.text = message;
        // Wait one frame before allowing message dismissal.
        yield return null;
        yield return _waitUntilDismissed;
        InterpretAndHandleDismissal();
        displayingMessage = false;
        canvasGroup.alpha = 0;
    }

    public IEnumerator DisplayDialogues(params Vector2Int[] sceneKeys)
    {
        StartConversation();
        foreach (var sceneKey in sceneKeys)
        {
            yield return DisplayDialogue(new Vector2Int(sceneKey.x, sceneKey.y));
        }

        EndConversation();
    }

    public IEnumerator DisplayDialoguesFromScene(int sceneKey)
    {
        var sceneLength = CustomProjectSettings.i.dialogueDatabaseAsset.scenes[sceneKey].dialogues.Count;
        StartConversation();
        for (int j = 0; j < sceneLength; j++)
        {
            yield return DisplayDialogue(new Vector2Int(sceneKey, j));
        }

        EndConversation();
    }

    private bool IsDismissalRequested()
    {
        return _input.dialogueDismissed || networkedDismissalRequests > 0;
    }

    private void DecrementNetworkedDismissalRequests()
    {
        if (networkedDismissalRequests > 0)
        {
            networkedDismissalRequests -= 1;
        }
    }

    private void InterpretAndHandleDismissal()
    {
        if (networkedDismissalRequests > 0)
        {
            networkedDismissalRequests -= 1;
        }
        else if (_input.dialogueDismissed)
        {
            // Make other clients dismiss their typing.
        }
    }

    public void SendRPCDismissDialogue()
    {
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC(nameof(RPCDismissDialogue), RpcTarget.Others, typing);
        }
    }

    [PunRPC]
    public void RPCDismissDialogue(bool dismissTyping)
    {
        if (dismissTyping && !typing) return;
        networkedDismissalRequests += 1;
    }
}