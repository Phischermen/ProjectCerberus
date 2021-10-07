/*
 * This is the script present in the initial scene loaded when our game starts. This scene will likely be our splash
 * screen and logos etc.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitialSceneController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SceneManager.LoadScene((int) Scenum.Scene.JackTutorial);
    }
}
