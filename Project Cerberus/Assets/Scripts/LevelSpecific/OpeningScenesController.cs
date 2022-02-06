using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpeningScenesController : MonoBehaviour
{
    public int openingSequence;
    private bool _triggerTripped1;
    private bool _triggerTripped2;

    public IEnumerator Start()
    {
        if (openingSequence == 1)
        {
            yield return null;
            var jack = FindObjectOfType<Jack>();

            DialoguePanel.i.StartConversation();
            jack.PlayAnimation(jack.Talk(1f, 0.5f));
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.Jou1);
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.Jo15);
            jack.FinishCurrentAnimation();
            DialoguePanel.i.EndConversation();
        }
        else if (openingSequence == 2)
        {
            yield return null;
            var jack = FindObjectOfType<Jack>();
            var kahuna = FindObjectOfType<Kahuna>();
            var laguna = FindObjectOfType<Laguna>();
            
            DialoguePanel.i.StartConversation();
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.ghJ1);
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.ghL2);
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.ghJ3);
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.ghK4);
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.ghJ5);
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.ghJ6);
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.ghL7);
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.ghL8);
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.ghJ9);
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.gJ10);
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.gL11);
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.gJ12);
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.gJ13);
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.gK14);
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.gL15);
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.gJ16);
            DialoguePanel.i.EndConversation();
        }
    }

    public IEnumerator OnEnterOfficeCutscene()
    {
        yield return null;
        var jack = FindObjectOfType<Jack>();

        DialoguePanel.i.StartConversation();
        jack.PlayAnimation(jack.Talk(1f, 0.5f));
        yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.Jio1);
        yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.Jio2);
        jack.FinishCurrentAnimation();
        DialoguePanel.i.EndConversation();
    }

    public IEnumerator OnEnterBathroomCutscene()
    {
        yield return null;
        Debug.Log("Bathroom cutscene");
    }

    public void PlayOnEnterOfficeCutscene()
    {
        if (!_triggerTripped1)
        {
            StartCoroutine(OnEnterOfficeCutscene());
        }

        _triggerTripped1 = true;
    }

    public void PlayOnEnterBathroomCutscene()
    {
        if (!_triggerTripped2)
        {
            StartCoroutine(OnEnterBathroomCutscene());
        }

        _triggerTripped2 = true;
    }
}