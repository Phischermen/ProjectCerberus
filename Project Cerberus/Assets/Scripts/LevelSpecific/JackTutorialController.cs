using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JackTutorialController : MonoBehaviour
{
    public Transform returnPosition;
    public Transform block1;
    public Transform block2;

    public AnimationCurve cameraInterpolationOverTime;
    public float interpolationDuration;

    public void PlayFollowBlock1Cutscene()
    {
        StartCoroutine(FollowBlockCutscene(block1));
    }
    
    public void PlayFollowBlock2Cutscene()
    {
        StartCoroutine(FollowBlockCutscene(block2));
    }

    IEnumerator FollowBlockCutscene(Transform blockTransform)
    {
        var timeEllapsed = 0f;
        while (timeEllapsed < interpolationDuration)
        {
            var interpolation = timeEllapsed / interpolationDuration;
            timeEllapsed += Time.deltaTime;

            var interpolation2 = cameraInterpolationOverTime.Evaluate(interpolation);

            PuzzleCameraController.SetPosition(Vector3.Lerp(returnPosition.position, blockTransform.position,
                interpolation2));
            yield return new WaitForFixedUpdate();
        }

        var jack = FindObjectOfType<Jack>();
        
        jack.PlayAnimation(jack.Talk(1f, 0.5f));
        Debug.Log("Jack says: I hit the wall every time!");
        yield return new WaitForSeconds(1f);
        jack.FinishCurrentAnimation();
        
        Debug.Log("Cutscene finished");
    }
}