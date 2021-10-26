using UnityEditor;
using UnityEngine;

namespace Editor
{
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

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Connect to gate"))
            {
                GenericMenu menu = new GenericMenu();
                foreach (var gate in FindObjectsOfType<Gate>())
                {
                    menu.AddItem(new GUIContent(gate.name), false, OnSelectItem, gate);
                }

                menu.ShowAsContext();
            }
        }

        private void OnSelectItem(object userdata)
        {
            var counter = (Counter) target;
            if (userdata is Gate gate)
            {
                UnityEditor.Events.UnityEventTools.AddPersistentListener(counter.OnCounterEqualsZero, gate.OpenGate);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(counter.OnCounterNoLongerEqualsZero,
                    gate.RequestCloseGate);
                EditorUtility.SetDirty(target);
            }
            else if (userdata is Counter counter1)
            {
                UnityEditor.Events.UnityEventTools.AddPersistentListener(counter.OnCounterEqualsZero,
                    counter1.IncrementCounter);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(counter.OnCounterNoLongerEqualsZero,
                    counter1.DecrementCounter);
                EditorUtility.SetDirty(target);
            }
        }
    }
}