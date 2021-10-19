/*
 * A custom editor for level sequence.
 */

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(LevelSequence))]
    public class LevelSequenceEditor : UnityEditor.Editor
    {
        private int _editedWorld;
        private int _editedLevel;
        public override void OnInspectorGUI()
        {
            var levelSequence = (LevelSequence) target;
            GUILayout.Label("Level Sequence");
            // Iterate through every world.
            for (var i = 0; i < levelSequence.worlds.Count; i++)
            {
                var world = levelSequence.worlds[i];
                // Add foldout group to display world.
                world.expandedInInspector =
                    EditorGUILayout.BeginFoldoutHeaderGroup(world.expandedInInspector, $"World{i}");
                if (world.expandedInInspector)
                {
                    // Add button to delete world.
                    if (GUILayout.Button("Delete World"))
                    {
                        // Make this action undoable, and let Zach know he can undo.
                        UnityEditor.Undo.RecordObject(levelSequence, "Delete World");
                        NZ.NotifyZach("You deleted their world! Ctrl+Z to undo");
                        // Remove the world
                        levelSequence.worlds.Remove(world);
                    }

                    // Iterate through levels.
                    for (var j = 0; j < world.levels.Count; j++)
                    {
                        var scene = world.levels[j];
                        // Each level has a row of controls. Start building here.
                        GUILayout.BeginHorizontal();
                        var path = EditorBuildSettings.scenes[scene].path;
                        EditorGUILayout.LabelField(path);
                        // Add edit button
                        if (GUILayout.Button("Open"))
                        {
                            EditorSceneManager.OpenScene(path);
                        }
                        // Add move up button
                        if (GUILayout.Button("▲") && j != 0)
                        {
                            // Move the scene "up" the list.
                            world.levels.RemoveAt(j);
                            world.levels.Insert(j - 1, scene);
                        }

                        // Add move down button.
                        if (GUILayout.Button("▼") && j != world.levels.Count)
                        {
                            // Move the scene "down" the list.
                            world.levels.RemoveAt(j);
                            world.levels.Insert(j + 1, scene);
                        }
                        // Add insert button.
                        if (GUILayout.Button("Ins"))
                        {
                            // Display select menu, and insert the scene at the current index.
                            _editedWorld = i;
                            _editedLevel = j;
                            DisplaySelectMenu();
                        }
                        // Add delete button.
                        if (GUILayout.Button("✖"))
                        {
                            // Make delete undoable.
                            UnityEditor.Undo.RecordObject(levelSequence, "Delete Scene");
                            // Remove the scene
                            world.levels.RemoveAt(j);
                        }
                        // End row of buttons.
                        GUILayout.EndHorizontal();
                    }
                } // End of displaying level controls.
                // Add add scene button.
                if (GUILayout.Button("Add Scene"))
                {
                    // Display select menu, and append the scene to the bottom of the list.
                    _editedWorld = i;
                    _editedLevel = world.levels.Count;
                    DisplaySelectMenu();
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }// End of displaying world controls
            // Add add world button
            if (GUILayout.Button("Add World"))
            {
                // Add the world.
                levelSequence.AddWorld();
            }

            if (GUI.changed)
            {
                // Apply changes.
                EditorUtility.SetDirty(levelSequence);
            }
        }

        private void DisplaySelectMenu()
        {
            // Initialize generic menu.
            GenericMenu menu = new GenericMenu();
            // Populate generic menu with scene names from EditorBuildSettings.
            for (var index = 0; index < EditorBuildSettings.scenes.Length; index++)
            {
                var scene = EditorBuildSettings.scenes[index];
                // Get scene path.
                var name = scene.path;
                // Extract name from the full path.
                name = name.Remove(name.LastIndexOf('.'));
                name = name.Remove(0, name.LastIndexOf('/') + 1);
                // Add the item to the menu. OnSelectScene is the callback.
                menu.AddItem(new GUIContent(name), false, OnSelectScene, index);
            }

            // Show menu.
            menu.ShowAsContext();
        }

        private void OnSelectScene(object userdata)
        {
            // Cast objects.
            var levelSequence = (LevelSequence) target;
            var index = (int) userdata;
            // Insert the selected scene into the edited field.
            levelSequence.worlds[_editedWorld].levels
                .Insert(_editedLevel, index);
        }
    }
}