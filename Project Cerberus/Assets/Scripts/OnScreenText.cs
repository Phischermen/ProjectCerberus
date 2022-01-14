/*
 * OnScreenText is designed for putting placeholder text on screen. I'm using it to implement basic tutorials as well as
 * personal notes and TODOs for levels. They can be hidden in game by clicking them.
 */

using UnityEngine;

[ExecuteAlways]
public class OnScreenText : MonoBehaviour
{
    private static GUIStyle _style;
    private static GUIContent _hiddenContent;
    private GUIContent _content;

    public Camera mainCamera;
    [TextArea] public string message;
    public float width = 200f;
    private bool _hidden;

#if !UNITY_WEBGL
    public void OnGUI()
    {
        VerifyStyleAndContent();
        var chosenContent = _hidden ? _hiddenContent : _content;
        var chosenWidth = _hidden ? 50f : width;
        var calcHeight = _style.CalcHeight(chosenContent, chosenWidth);
        var screenPoint = mainCamera.WorldToScreenPoint(transform.position);
        if (GUI.Button(new Rect(screenPoint.x, Screen.height - screenPoint.y, chosenWidth, calcHeight),
            chosenContent, _style))
        {
            _hidden = !_hidden;
        }
    }
#endif
    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void VerifyStyleAndContent()
    {
        if (_style == null)
        {
            _style = new GUIStyle(GUI.skin.button) {fontSize = 24, wordWrap = true};
        }

        if (_content == null)
        {
            _content = new GUIContent(message);
        }

        if (_hiddenContent == null)
        {
            _hiddenContent = new GUIContent("...");
        }
    }
}