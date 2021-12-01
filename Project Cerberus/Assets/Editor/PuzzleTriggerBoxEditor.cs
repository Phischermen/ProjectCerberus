/*
 * Custom Editor for PuzzleTriggerBoxEditor.
 */

using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(PuzzleTriggerBoxEditor))]
    public class PuzzleTriggerBoxEditor : UnityEditor.Editor
    {
        private static GUIStyle _triggerStyle;

        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
        static void RenderGizmo(PuzzleTriggerBox puzzleTriggerBox, GizmoType gizmoType)
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

            var cellSize = FindObjectOfType<Grid>().cellSize;
            var position = puzzleTriggerBox.transform.position - cellSize / 2;
            var isSelected = ((int) gizmoType & (int) GizmoType.Selected) != 0;
            Handles.DrawSolidRectangleWithOutline(new Rect(position, cellSize),
                new Color(0, 1, 0, isSelected ? 0.5f : 0.05f), Color.green);
            Handles.Label(position, puzzleTriggerBox.name, _triggerStyle);
        }
    }
}