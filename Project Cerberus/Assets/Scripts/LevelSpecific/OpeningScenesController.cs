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
        // Check if player wants to skip story.
        if (MainMenuController.silenceStory)
        {
            yield break;
        }
        if (openingSequence == 1)
        {
            yield return null;
            var jack = FindObjectOfType<Jack>();

            DialoguePanel.i.StartConversation();
            jack.PlayAnimation(jack.Talk(1f, 0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.Jou1);
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.Jo15);
            jack.FinishCurrentAnimation();
            DialoguePanel.i.EndConversation();
        }
        else if (openingSequence == 2)
        {
            yield return null;
            var finish = FindObjectOfType<Finish>();
            
            // Hide finish
            finish.SetCollisionsEnabled(false);
            finish.LookUnavailable();
        }
        else if (openingSequence == 3)
        {
            yield return null;
            var jack = FindObjectOfType<Jack>();
            var kahuna = FindObjectOfType<Kahuna>();
            var laguna = FindObjectOfType<Laguna>();
            
            DialoguePanel.i.StartConversation();
            jack.PlayAnimation(jack.Talk(1f, 0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.ghJ1);
            jack.FinishCurrentAnimation();
            laguna.PlayAnimation(laguna.Talk(1f,0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.ghL2);
            laguna.FinishCurrentAnimation();
            jack.PlayAnimation(jack.Talk(1f, 0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.ghJ3);
            jack.FinishCurrentAnimation();
            kahuna.PlayAnimation(kahuna.Talk(1f, 0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.ghK4);
            kahuna.FinishCurrentAnimation();
            jack.PlayAnimation(jack.Talk(1f, 0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.ghJ5);
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.ghJ6);
            jack.FinishCurrentAnimation();
            laguna.PlayAnimation(laguna.Talk(1f,0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.ghL7);
            laguna.FinishCurrentAnimation();
            kahuna.PlayAnimation(kahuna.Talk(1f, 0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.ghL8);
            kahuna.FinishCurrentAnimation();
            jack.PlayAnimation(jack.Talk(1f, 0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.ghJ9);
            jack.FinishCurrentAnimation();
            // TODO GASPing animation.
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.gJ10);
            laguna.PlayAnimation(laguna.Talk(1f,0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.gL11);
            laguna.FinishCurrentAnimation();
            jack.PlayAnimation(jack.Talk(1f, 0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.gJ12);
            jack.FinishCurrentAnimation();
            // TODO Thinking animation.
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.gJ13);
            kahuna.PlayAnimation(kahuna.Talk(1f, 0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.gK14);
            kahuna.FinishCurrentAnimation();
            laguna.PlayAnimation(laguna.Talk(1f,0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.gL15);
            laguna.FinishCurrentAnimation();
            jack.PlayAnimation(jack.Talk(1f, 0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.gJ16);
            jack.FinishCurrentAnimation();
            DialoguePanel.i.EndConversation();
        }
    }

    public IEnumerator OnEnterOfficeCutscene()
    {
        yield return null;
        var jack = FindObjectOfType<Jack>();

        DialoguePanel.i.StartConversation();
        jack.PlayAnimation(jack.Talk(1f, 0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
        yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.Jio1);
        yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.Jio2);
        jack.FinishCurrentAnimation();
        DialoguePanel.i.EndConversation();
    }

    public IEnumerator OnEnterBathroomCutscene()
    {
        yield return null;
        var jack = FindObjectOfType<Jack>();
        var hades = FindObjectOfType<Hades>();
        
        // TODO Reveal bathroom.
        DialoguePanel.i.StartConversation();
        // This section is skipped if the player wants to silence story.
        if (!MainMenuController.silenceStory)
        {
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.jBr1);
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.jBr2);
            hades.PlayAnimation(hades.Talk(1f, 0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.jBr3);
            hades.FinishCurrentAnimation();
            jack.PlayAnimation(jack.Talk(1f, 0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.jBr4);
            jack.FinishCurrentAnimation();
            hades.PlayAnimation(hades.Talk(1f, 0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.jBr5);
            hades.FinishCurrentAnimation();
            jack.PlayAnimation(jack.Talk(1f, 0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.jBr6);
            jack.FinishCurrentAnimation();
            hades.PlayAnimation(hades.Talk(1f, 0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.jBr7);
            hades.FinishCurrentAnimation();
            jack.PlayAnimation(jack.Talk(1f, 0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.jBr8);
            jack.FinishCurrentAnimation();
            hades.PlayAnimation(hades.Talk(1f, 0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.jBr9);
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.jB10);
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.jB11);
        }
        jack.BasicMove(Vector2Int.right);
        yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.jB12);
        hades.MoveForCutscene(Vector2Int.right);
        jack.BasicMove(Vector2Int.right);
        yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.jB13);
        hades.MoveForCutscene(Vector2Int.right);
        jack.BasicMove(Vector2Int.right);
        yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.jB14);
        hades.MoveForCutscene(Vector2Int.right);
        jack.BasicMove(Vector2Int.right);
        yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.jB15);
        hades.FinishCurrentAnimation();
        DialoguePanel.i.EndConversation();
        hades.chaseEntityEnabled = true;
        // Show finish
        var finish = FindObjectOfType<Finish>();
        finish.LookAvailable();
        finish.SetCollisionsEnabled(true);
    }

    public void PlayOnEnterOfficeCutscene()
    {
        if (!_triggerTripped1 && !MainMenuController.silenceStory)
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