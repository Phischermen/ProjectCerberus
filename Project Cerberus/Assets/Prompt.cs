using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prompt : MonoBehaviour
{
    private static bool _promptIsDisplayed;
    [TextArea] public string contents = "";

    public void DisplayPrompt()
    {
        if (!_promptIsDisplayed)
        {
            _promptIsDisplayed = true;
            // TODO Connect to UI
        }
    }

    public void HidePrompt()
    {
        if (_promptIsDisplayed)
        {
            _promptIsDisplayed = false;
        }
    }

    // Communicate player obeyed prompt
    public void PromptFlashPositive()
    {
        Debug.Log($"{name} : Positive Flash");
    }

    // Communicate player disobeyed prompt
    public void PromptFlashNegative()
    {
        Debug.Log($"{name} : Negative Flash");
    }
}