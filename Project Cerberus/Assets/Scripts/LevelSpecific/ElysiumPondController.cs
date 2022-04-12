using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ElysiumPondController : MonoBehaviour
{
    public int sequence;

    private GUIStyle _triggerStyle;

    private Hades _hades;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        if (sequence == 1)
        {
            _hades = FindObjectOfType<Hades>();
            _hades.entityToChase = FindObjectOfType<CerberusMajor>();
            _hades.chaseEntityEnabled = false;
            _hades.gameObject.SetActive(false);
            // TODO display some dialogue.
        }

        return null;
    }

    public void OnCerberusEnterTrigger()
    {
        _hades.chaseEntityEnabled = true;
        _hades.gameObject.SetActive(true);
        _hades.onCatchTarget.AddListener(OnHadesCatchesTarget);
        _hades.onBecomeTrapped.AddListener(OnHadesTrapped);
        Debug.Log("Hades comes out of the bushes.");
    }

    public void OnHadesCatchesTarget()
    {
        FindObjectOfType<GameManager>().EndGameWithFailureStatus();
        // var jumpPoints = [] {new CerberusMajor.JumpInfo()}
        // FindObjectOfType<CerberusMajor>().JumpAlongPath(jum, AnimationUtility.jumpSpeed);
    }

    public void OnHadesTrapped()
    {
        Debug.Log("Hades screams in agony and gives up!");
        _hades.chaseEntityEnabled = false;
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