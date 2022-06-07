using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChainJumpPrompter : MonoBehaviour
{
    private CerberusMajor _cerberusMajor;

    private int _numberOfSingleJumpsInARow;
    private int _thresholdToPrompt = 3;
    private static bool _hasPrompted;

    // Start is called before the first frame update
    void Start()
    {
        _cerberusMajor = FindObjectOfType<CerberusMajor>();
        if (_cerberusMajor)
        {
            _cerberusMajor.onSingleJumpCompleted.AddListener(PromptPlayerToUseChainJump);
            _cerberusMajor.onStandardMove.AddListener(ResetCounter);
        }
    }

    private void PromptPlayerToUseChainJump()
    {
        _numberOfSingleJumpsInARow += 1;
        if (_numberOfSingleJumpsInARow > _thresholdToPrompt)
        {
            if (!_hasPrompted)
            {
                StartCoroutine(DialoguePanel.i.DisplayDialoguesFromScene(DialogueDatabase.cj01.x));
                _hasPrompted = true;
            }
            else
            {
                var popups = new[]
                {
                    TextPopup.Create("Don't forget to chain jumps!", Color.yellow),
                    TextPopup.Create("Hold LEFT SHIFT and press your MOVE KEYS.", Color.yellow)
                };
                for (var i = 0; i < popups.Length; i++)
                {
                    var popup = popups[i];
                    var textMeshPro = popup.GetComponent<TextMeshPro>();
                    textMeshPro.enableAutoSizing = false;
                    textMeshPro.enableWordWrapping = false;
                    // Hide popups until they're ready to be shown.
                    var color = textMeshPro.color;
                    color.a = 0f;
                    textMeshPro.color = color;
                    popup.transform.position = _cerberusMajor.transform.position;
                    popup.PlayRiseAndFadeAnimation(i * 1.5f);
                }
            }
            _thresholdToPrompt = _thresholdToPrompt * 2;
        }
    }

    public void ResetCounter()
    {
        _numberOfSingleJumpsInARow = 0;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawIcon(new Vector3(0,2,0), "Jump Ropes");
    }
}