/*
 * NOTE: This collection of sequences uses new methods I wrote later in the project's lifecycle. I don't plan on going
 * back to refactor BasicTutorialController.cs to use my new methods, since it'd take too much time.
 */

using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class CerberusTutorialController : MonoBehaviour
{
    public int tutorialSequence;

    [Header("Sequence 2")] public BasicBlock purpleBlock;

    public Transform spikes;
    public Transform pit;
    private GUIStyle _triggerStyle;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        var pointer = FindObjectOfType<Pointer>();
        if (pointer)
        {
            pointer.gameObject.SetActive(false);
        }
        // Check if player wants to skip tutorials.
        if (MainMenuController.silenceTutorials)
        {
            yield break;
        }

        yield return null;
        if (tutorialSequence == 1)
        {
            yield return DialoguePanel.i.DisplayDialoguesFromScene(DialogueDatabase.ct01.x);
        }

        if (tutorialSequence == 2)
        {
            DialoguePanel.i.StartConversation();
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.ct04);
            pointer.gameObject.SetActive(true);
            pointer.position = purpleBlock.transform.position;
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.ct05);
            pointer.position = spikes.position;
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.ct06);
            pointer.position = pit.position;
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.ct07);
            pointer.gameObject.SetActive(false);
            DialoguePanel.i.EndConversation();
        }

        if (tutorialSequence == 3)
        {
            yield return DialoguePanel.i.DisplayDialoguesFromScene(DialogueDatabase.cta1.x);
        }

        if (tutorialSequence == 4)
        {
            var cerberusMajor = FindObjectOfType<CerberusMajor>();
            var finish = FindObjectOfType<Finish>();
            DialoguePanel.i.StartConversation();
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.ct08);
            pointer.gameObject.SetActive(true);
            pointer.position = cerberusMajor.transform.position;
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.ct09);
            pointer.gameObject.SetActive(false);
            yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.ct10);
            pointer.gameObject.SetActive(true);
            pointer.position = finish.transform.position;
            yield return DialoguePanel.i.DisplayDialogues(DialogueDatabase.ct11, DialogueDatabase.ct12);
            pointer.gameObject.SetActive(false);
            DialoguePanel.i.EndConversation();
        }
    }

    private void OnDrawGizmos()
    {
        if (_triggerStyle == null)
        {
            _triggerStyle = new GUIStyle()
            {
                normal = new GUIStyleState()
                {
                    background = Texture2D.grayTexture,
                    textColor = Color.white
                }
            };
        }
        Gizmos.DrawIcon(transform.position, "Film Marker");
        Handles.Label(transform.position, tutorialSequence.ToString(), _triggerStyle);
    }
}