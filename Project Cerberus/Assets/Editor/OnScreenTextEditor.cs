using System;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(OnScreenText))]
    public class OnScreenTextEditor : UnityEditor.Editor
    {
        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
        static void RenderGizmo(OnScreenText onScreenText, GizmoType gizmoType)
        {
            Handles.FreeMoveHandle(onScreenText.transform.position, Quaternion.identity, 0.5f, Vector3.zero, Handles.CircleHandleCap);
        }

        private void OnSceneGUI()
        {
            var onScreenText = (OnScreenText) target;
            EditorGUI.BeginChangeCheck();
            var newPosition = Handles.FreeMoveHandle(onScreenText.transform.position, Quaternion.identity, 0.5f, Vector3.zero, Handles.CircleHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                onScreenText.transform.position = newPosition;
            }
        }
    }
}
