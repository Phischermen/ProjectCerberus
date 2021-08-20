using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Counter))]
public class CounterEditor : UnityEditor.Editor
{
    // I wish this was actually pickable but Unity makes it too inconvenient to set that up!
    [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
    static void RenderGizmo(Counter counter, GizmoType gizmoType)
    {
        var content = new GUIContent($"{counter.name}\nCount: {counter.initialValue}");
        GUIStyle style = GUI.skin.box;
        var position = counter.transform.position;
        Handles.Label(position, content, style);
    }
}