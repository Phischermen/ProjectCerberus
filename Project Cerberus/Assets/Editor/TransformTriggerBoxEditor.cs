/*
 * Custom Editor for TransformTriggerBox.
 */

using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(TransformTriggerBox))]
    public class TransformTriggerBoxEditor : UnityEditor.Editor
    {
        private BoxBoundsHandle _boxBoundsHandle = new BoxBoundsHandle();

        private static GUIStyle _triggerStyle;

        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
        static void RenderGizmo(TransformTriggerBox transformTriggerBox, GizmoType gizmoType)
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

            var position = transformTriggerBox.bounds.center - transformTriggerBox.bounds.extents;
            var isSelected = ((int) gizmoType & (int) GizmoType.Selected) != 0;
            Handles.DrawSolidRectangleWithOutline(new Rect(position, transformTriggerBox.bounds.extents * 2),
                new Color(1, 0, 0, isSelected ? 0.5f : 0.05f), Color.red);
            Handles.Label(position, transformTriggerBox.name, _triggerStyle);
        }

        protected virtual void OnSceneGUI()
        {
            TransformTriggerBox transformTriggerBox = (TransformTriggerBox) target;

            // Copy the target object's data to the handle.
            _boxBoundsHandle.center = transformTriggerBox.bounds.center;
            _boxBoundsHandle.size = transformTriggerBox.bounds.size;

            // Draw the handle.
            EditorGUI.BeginChangeCheck();
            _boxBoundsHandle.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                // Copy the handle's updated data back to the target object.
                Bounds newBounds = new Bounds();
                newBounds.center = _boxBoundsHandle.center;
                newBounds.size = _boxBoundsHandle.size;
                transformTriggerBox.bounds = newBounds;
            }
        }
    }
}