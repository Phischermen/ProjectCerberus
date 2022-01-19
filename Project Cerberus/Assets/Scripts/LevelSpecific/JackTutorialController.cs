using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JackTutorialController : MonoBehaviour
{
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
        var returnPosition = PuzzleCameraController.i.GetPosition();
        var originalMode = PuzzleCameraController.i.currentCameraMode;
        PuzzleCameraController.i.SetCameraMode(PuzzleCameraController.CameraMode.CinematicMode);
        var timeEllapsed = 0f;
        while (timeEllapsed < interpolationDuration)
        {
            var interpolation = timeEllapsed / interpolationDuration;
            timeEllapsed += Time.deltaTime;

            var interpolation2 = cameraInterpolationOverTime.Evaluate(interpolation);

            PuzzleCameraController.i.SetPosition(Vector3.Lerp(returnPosition, blockTransform.position,
                interpolation2));
            yield return new WaitForFixedUpdate();
        }
        PuzzleCameraController.i.SetCameraMode(originalMode);

        var jack = FindObjectOfType<Jack>();
        
        jack.PlayAnimation(jack.Talk(1f, 0.5f));
        var gameManager = FindObjectOfType<GameManager>();
        gameManager.gameplayEnabled = false;
        yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.I2D2);
        yield return DialoguePanel.i.DisplayDialogue(DialogueDatabase.M8F6);
        gameManager.gameplayEnabled = true;
        yield return new WaitForSeconds(1f);
        jack.FinishCurrentAnimation();
        
        Debug.Log("Cutscene finished");
    }
}