using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicsTutorialController : MonoBehaviour
{
    public int tutorialSequence;
    [Header("Sequence 3")] public Switch mySwitch;

    public BasicBlock block;
    [Header("Sequence 5")] public Switch mySwitch1;

    public BasicBlock block1;
    public Transform spikes;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        var pointer = FindObjectOfType<Pointer>();
        // Check if player wants to skip tutorials.
        if (MainMenuController.silenceTutorials)
        {
            if (pointer)
            {
                pointer.gameObject.SetActive(false);
            }

            yield break;
        }

        if (tutorialSequence == 1)
        {
            yield return null;

            var jack = FindObjectOfType<Jack>();
            var star = FindObjectOfType<BonusStar>();
            var finish = FindObjectOfType<Finish>();
            DialoguePanel.i.StartConversation();
            pointer.position = jack.transform.position;
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.W4H2);
            pointer.gameObject.SetActive(false);
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.M8T4);
            pointer.gameObject.SetActive(true);
            pointer.position = finish.transform.position;
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.F3C6);
            pointer.position = star.transform.position;
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.M7Q2);
            pointer.gameObject.SetActive(false);
            DialoguePanel.i.EndConversation();
        }
        else if (tutorialSequence == 2)
        {
            yield return null;
            var jack = FindObjectOfType<Jack>();
            var kahuna = FindObjectOfType<Kahuna>();
            var laguna = FindObjectOfType<Laguna>();
            pointer.gameObject.SetActive(false);
            DialoguePanel.i.StartConversation();
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.T331);
            kahuna.PlayAnimation(kahuna.Talk(1f, 0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.T332);
            kahuna.FinishCurrentAnimation();
            laguna.PlayAnimation(laguna.Talk(1f, 0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.T333);
            laguna.FinishCurrentAnimation();
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.t334);
            pointer.gameObject.SetActive(true);
            pointer.position = jack.transform.position;
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.t335);
            pointer.gameObject.SetActive(false);
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.t336);
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.t337);
            DialoguePanel.i.EndConversation();
        }
        else if (tutorialSequence == 3)
        {
            yield return null;
            pointer.gameObject.SetActive(false);
            DialoguePanel.i.StartConversation();
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.s1J1);
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.s1J2);
            pointer.gameObject.SetActive(true);
            pointer.position = block.transform.position;
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.s1J3);
            pointer.position = mySwitch.transform.position;
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.s1J4);
            pointer.gameObject.SetActive(false);
            DialoguePanel.i.EndConversation();
        }
        else if (tutorialSequence == 4)
        {
            yield return null;
            DialoguePanel.i.StartConversation();
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.s2K1);
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.s2K2);
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.s2K3);
            DialoguePanel.i.EndConversation();
        }
        else if (tutorialSequence == 5)
        {
            yield return null;
            pointer.gameObject.SetActive(false);
            DialoguePanel.i.StartConversation();
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.s3L1);
            pointer.gameObject.SetActive(true);
            pointer.position = block1.transform.position;
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.s3L2);
            pointer.position = spikes.position;
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.s3L3);
            pointer.position = mySwitch1.transform.position;
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.s3L4);
            pointer.gameObject.SetActive(false);
            DialoguePanel.i.EndConversation();
        }
        else if (tutorialSequence == 6)
        {
            yield return null;
            var star = FindObjectOfType<BonusStar>();
            pointer.gameObject.SetActive(false);
            DialoguePanel.i.StartConversation();
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.aLL1);
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.aLL2);
            pointer.gameObject.SetActive(true);
            pointer.position = star.transform.position;
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.aLL3);
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.aLL4);
            pointer.gameObject.SetActive(false);
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.aLL5);
            DialoguePanel.i.EndConversation();
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}