using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(Switch), true)]
    public class SwitchEditor : PuzzleEntityEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Connect to gate"))
            {
                GenericMenu menu = new GenericMenu();
                foreach (var gate in FindObjectsOfType<Gate>())
                {
                    menu.AddItem(new GUIContent(gate.name), false, OnSelectItem, gate);
                }

                menu.ShowAsContext();
            }
            if (GUILayout.Button("Connect to counter"))
            {
                GenericMenu menu = new GenericMenu();
                foreach (var gate in FindObjectsOfType<Counter>())
                {
                    menu.AddItem(new GUIContent(gate.name), false, OnSelectItem, gate);
                }

                menu.ShowAsContext();
            }
        }

        private void OnSelectItem(object userdata)
        {
            var @switch = (Switch) target;
            if (userdata is Gate gate)
            {
                UnityEditor.Events.UnityEventTools.AddPersistentListener(@switch.onPressed, gate.OpenGate);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(@switch.onReleased, gate.RequestCloseGate);
                EditorUtility.SetDirty(target);
            }
            else if (userdata is Counter counter)
            {
                UnityEditor.Events.UnityEventTools.AddPersistentListener(@switch.onPressed, counter.IncrementCounter);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(@switch.onReleased, counter.DecrementCounter);
                EditorUtility.SetDirty(target);
            }
        }
    }
}