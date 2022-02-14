/*
 * A custom editor for level sequence.
 */

using System;
using System.IO;
using System.Linq;
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

        private static GUIStyle _normalStyle;
        private static GUIStyle _sceneMissingStyle;

        private static GUIStyle _normalButtonStyle;
        private static GUIStyle _sceneMissingButtonStyle;

        private static double _timeForNextValidation;

        public override void OnInspectorGUI()
        {
            // Initialize styles
            if (_normalStyle == null)
            {
                _normalStyle = new GUIStyle(GUI.skin.label)
                {
                    wordWrap = false
                };
                _sceneMissingStyle = new GUIStyle(GUI.skin.label)
                {
                    normal = new GUIStyleState()
                    {
                        textColor = Color.red
                    },
                    wordWrap = false
                };

                _normalButtonStyle = new GUIStyle(GUI.skin.button);
                _sceneMissingButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    normal = new GUIStyleState()
                    {
                        textColor = Color.red
                    },
                    hover = new GUIStyleState()
                    {
                        textColor = Color.red
                    }
                    
                    
                };
            }

            // Proceed with InspectorGUI
            var levelSequence = (LevelSequence) target;
            GUILayout.Label("Level Sequence");
            // Add button for manual validation
            if (GUILayout.Button("Validate"))
            {
                ValidateLevelSequence();
            }
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

                    // Add buttons to change order of worlds
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("▲") && i != 0)
                    {
                        // Make move up undoable
                        UnityEditor.Undo.RecordObject(levelSequence, "Move Up");
                        // Move world "up" the list.
                        levelSequence.worlds.RemoveAt(i);
                        levelSequence.worlds.Insert(i - 1, world);
                    }

                    if (GUILayout.Button("▼") && i != levelSequence.worlds.Count - 1)
                    {
                        // Make move down undoable
                        UnityEditor.Undo.RecordObject(levelSequence, "Move Down");
                        // Move world "up" the list.
                        levelSequence.worlds.RemoveAt(i);
                        levelSequence.worlds.Insert(i + 1, world);
                    }

                    EditorGUILayout.EndHorizontal();

                    // Iterate through levels.
                    for (var j = 0; j < world.levels.Count; j++)
                    {
                        var scene = world.levels[j];
                        // Each level has a row of controls. Start building here.
                        GUILayout.BeginHorizontal();
                        var buildSettingsScene = EditorBuildSettings.scenes[scene.y];
                        var path = buildSettingsScene.path;
                        var enabled = buildSettingsScene.enabled;
                        var sceneExists = File.Exists(path);
                        //EditorGUILayout.LabelField(sceneExists ? $"({scene.x},{scene.y}){path}" : "SCENE DELETED", enabled ? _normalStyle : _sceneMissingStyle);
                        EditorGUILayout.LabelField(sceneExists ? path : "SCENE DELETED", enabled ? _normalStyle : _sceneMissingStyle);
                        // Add edit button
                        if (GUILayout.Button("Open", sceneExists ? _normalButtonStyle : _sceneMissingButtonStyle))
                        {
                            if (sceneExists)
                            {
                                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                                {
                                    EditorSceneManager.OpenScene(path);
                                }
                            }
                        }

                        // Add move up button
                        if (GUILayout.Button("▲") && j != 0)
                        {
                            // Make move up undoable.
                            UnityEditor.Undo.RecordObject(levelSequence, "Move Up");
                            // Move the scene "up" the list.
                            world.levels.RemoveAt(j);
                            world.levels.Insert(j - 1, scene);
                        }

                        // Add move down button.
                        if (GUILayout.Button("▼") && j != world.levels.Count - 1)
                        {
                            // Make move down undoable.
                            UnityEditor.Undo.RecordObject(levelSequence, "Move Down");
                            // Move the scene "down" the list.
                            world.levels.RemoveAt(j);
                            world.levels.Insert(j + 1, scene);
                        }

                        // Add insert button.
                        if (GUILayout.Button("Ins"))
                        {
                            // Make insert undoable.
                            UnityEditor.Undo.RecordObject(levelSequence, "Insert Scene");
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
                    // Make add scene undoable.
                    UnityEditor.Undo.RecordObject(levelSequence, "Add Scene");
                    // Display select menu, and append the scene to the bottom of the list.
                    _editedWorld = i;
                    _editedLevel = world.levels.Count;
                    DisplaySelectMenu();
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            } // End of displaying world controls

            // Add add world button
            if (GUILayout.Button("Add World"))
            {
                // Make add world undoable.
                UnityEditor.Undo.RecordObject(levelSequence, "Add World");
                // Add the world.
                levelSequence.AddWorld();
            }

            if (GUI.changed)
            {
                // Apply changes.
                EditorUtility.SetDirty(levelSequence);
                ValidateLevelSequence();
            }
            // Validation
            if (_timeForNextValidation < EditorApplication.timeSinceStartup)
            {
                ValidateLevelSequence();
            }
        }
        
        private void ValidateLevelSequence()
        {
            _timeForNextValidation = EditorApplication.timeSinceStartup + 10.0;
            var levelSequence = (LevelSequence) target;
            // Ensure that 'index for instancing' is valid.
            for (var i = 0; i < levelSequence.worlds.Count; i++)
            {
                var world = levelSequence.worlds[i];
                for (var j = 0; j < world.levels.Count; j++)
                {
                    var level = world.levels[j];
                    // Bound check level.y
                    level.y = Mathf.Clamp(level.y, 0, EditorBuildSettings.scenes.Length);
                    // Verify level.x is the actual index to to instance scene.
                    var actualIndexToInstanceScene = EditorBuildSettings.scenes
                        .Where((scene, i1) => scene.enabled && i1 < level.y).Count();
                    if (level.x != actualIndexToInstanceScene)
                    {
                        level.x = actualIndexToInstanceScene;
                    }

                    world.levels[j] = level;
                }
            }
        }

        private void DisplaySelectMenu()
        {
            // Initialize generic menu.
            GenericMenu menu = new GenericMenu();
            // Populate generic menu with scene names from EditorBuildSettings, whilst searching for disabled scenes
            var indexToInstanceScene = 0;
            for (var index = 0; index < EditorBuildSettings.scenes.Length; index++)
            {
                var scene = EditorBuildSettings.scenes[index];
                if (scene.enabled)
                {
                    // Get scene path.
                    var name = scene.path;
                    // Extract name from the full path.
                    name = name.Remove(name.LastIndexOf('.'));
                    name = name.Remove(0, name.LastIndexOf('/') + 1);
                    // Add the item to the menu. OnSelectScene is the callback.
                    menu.AddItem(new GUIContent(name), false, OnSelectScene,
                        new Vector2Int(indexToInstanceScene, index));
                    indexToInstanceScene += 1;
                }
            }

            // Show menu.
            menu.ShowAsContext();
        }

        private void OnSelectScene(object userdata)
        {
            // Cast objects.
            var levelSequence = (LevelSequence) target;
            var tuple = (Vector2Int) userdata;
            // Insert the selected scene into the edited field.
            levelSequence.worlds[_editedWorld].levels
                .Insert(_editedLevel, tuple);
        }
    }
}