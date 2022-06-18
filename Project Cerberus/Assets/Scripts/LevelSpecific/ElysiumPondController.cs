using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ElysiumPondController : MonoBehaviour
{
    public int sequence;

    private GUIStyle _triggerStyle;

    private Hades _hades;
    private CerberusMajor _cerberusMajor;

    private bool _trigger1Tripped;
    private bool _trigger2Tripped;
    private bool _hadesAgonyTripped;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        if (sequence == 1)
        {
            _hades = FindObjectOfType<Hades>();
            _cerberusMajor = FindObjectOfType<CerberusMajor>();
            _hades.entityToChase = _cerberusMajor;
            _hades.chaseEntityEnabled = false;
            _hades.gameObject.SetActive(false);
            // TODO display some dialogue.
        }

        return null;
    }

    public void OnCerberusEnterTrigger()
    {
        if (!_trigger2Tripped)
        {
            _trigger2Tripped = true;
            _hades.gameObject.SetActive(true);
            Debug.Log("Hades comes out of the bushes.");
            StartCoroutine(HadesBeginsChase());
        }
    }

    public void OnCerberusEnterBushTrigger()
    {
        if (!_trigger1Tripped)
        {
            _trigger1Tripped = true;
            StartCoroutine(BushCutscene());
        }
    }

    public IEnumerator BushCutscene()
    {
        var cerberusMajor = FindObjectOfType<CerberusMajor>();

        DialoguePanel.i.StartConversation();
        cerberusMajor.PlayAnimation(cerberusMajor.Talk(1f, 0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
        yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.cre1);
        cerberusMajor.FinishCurrentAnimation();
        DialoguePanel.i.EndConversation();
    }

    public IEnumerator HadesBeginsChase()
    {
        DialoguePanel.i.StartConversation();
        _hades.PlayAnimation(_hades.Talk(1f, 0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
        yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.cre2);
        _hades.FinishCurrentAnimation();
        _cerberusMajor.PlayAnimation(_cerberusMajor.Talk(1f, 0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
        yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.cre3);
        _cerberusMajor.FinishCurrentAnimation();
        _cerberusMajor.PlayAnimation(_cerberusMajor.Talk(1f, 0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
        yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.cre4);
        _cerberusMajor.FinishCurrentAnimation();
        _cerberusMajor.PlayAnimation(_cerberusMajor.Talk(1f, 0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
        yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.cre5);
        _cerberusMajor.FinishCurrentAnimation();
        DialoguePanel.i.EndConversation();
        _hades.chaseEntityEnabled = true;
        _hades.onCatchTarget.AddListener(OnHadesCatchesTarget);
        _hades.onBecomeTrapped.AddListener(OnHadesTrapped);
    }

    public IEnumerator HadesScreamsInAgony()
    {
        DialoguePanel.i.StartConversation();
        _hades.PlayAnimation(_hades.Talk(1f, 0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
        yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.hsa1);
        _hades.FinishCurrentAnimation();
        _hades.PlayAnimation(_hades.Talk(1f, 0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
        yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.hsa2);
        _hades.FinishCurrentAnimation();
        _hades.PlayAnimation(_hades.Talk(1f, 0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
        yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.hsa3);
        _hades.FinishCurrentAnimation();
        _cerberusMajor.PlayAnimation(_cerberusMajor.Talk(1f, 0.5f, CustomProjectSettings.i.defaultTalkAnimationCurve));
        yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.hsa4);
        _cerberusMajor.FinishCurrentAnimation();
        DialoguePanel.i.EndConversation();
        yield return new WaitForSeconds(5f);
        FindObjectOfType<GameManager>().EndGameWithSuccessStatus();
    }

    public void OnHadesCatchesTarget()
    {
        FindObjectOfType<GameManager>().EndGameWithFailureStatus();
        // var jumpPoints = [] {new CerberusMajor.JumpInfo()}
        // FindObjectOfType<CerberusMajor>().JumpAlongPath(jum, AnimationUtility.jumpSpeed);
    }

    public void OnHadesTrapped()
    {
        _hades.chaseEntityEnabled = false;
        if (!_hadesAgonyTripped)
        {
            _hadesAgonyTripped = true;
            StartCoroutine(HadesScreamsInAgony());
        }
    }
#if UNITY_EDITOR
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

        Gizmos.DrawIcon(new Vector3(0, 1, 0), "Film Marker");
        Handles.Label(new Vector3(0, 1, 0), sequence.ToString(), _triggerStyle);
    }
#endif
}