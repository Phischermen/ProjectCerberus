/*
 * CreatePuzzleScene is used to create a level for our game in a quick and convenient way. It is accessible from the top
 * bar through "Tools > Create Puzzle Scene."
 */

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Editor
{
    public class CreatePuzzleScene : EditorWindow
    {
        private string _inputText;
        private bool[] _includes;
        private Vector2 _scrollPosition;

        [MenuItem("Tools/Create Puzzle Scene")]
        public static void CreatePuzzleSceneWindow()
        {
            var window = CreateWindow<CreatePuzzleScene>();
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
            window.ShowUtility();
        }

        private void OnBecameVisible()
        {
            // Instantiate includes.
            _includes = new bool[CustomProjectSettings.i.puzzleLevelIncludes.Length];
        }

        void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            // Add text field to enter scene name.
            EditorGUILayout.LabelField("Enter scene name:");
            _inputText = EditorGUILayout.TextField(_inputText);
            if (_inputText == "")
            {
                EditorGUILayout.LabelField("Scene name cannot be empty!");
            }

            // Add buttons to select game objects that will be included in the level.
            if (GUILayout.Button("Include All"))
            {
                for (int i = 0; i < _includes.Length; i++)
                {
                    _includes[i] = true;
                }
            }

            if (GUILayout.Button("Deselect All"))
            {
                for (int i = 0; i < _includes.Length; i++)
                {
                    _includes[i] = false;
                }
            }

            for (int i = 0; i < _includes.Length; i++)
            {
                _includes[i] =
                    EditorGUILayout.ToggleLeft(CustomProjectSettings.i.puzzleLevelIncludes[i].name, _includes[i]);
            }

            // Add button to create the level.
            if (GUILayout.Button("Create"))
            {
                if (File.Exists($"Assets/Scenes/{_inputText}.unity"))
                {
                    NZ.NotifyZach("A level with that name already exists");
                }
                else
                {
                    // Save the current scene.
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                    // Create the new scene.
                    var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                    // Add objects to the scene.
                    var puzzle =
                        (GameObject) PrefabUtility.InstantiatePrefab(CustomProjectSettings.i.puzzleContainerPrefab,
                            scene);
                    for (int i = 0; i < _includes.Length; i++)
                    {
                        if (_includes[i])
                        {
                            PrefabUtility.InstantiatePrefab(CustomProjectSettings.i.puzzleLevelIncludes[i],
                                puzzle.transform);
                        }
                    }

                    // Save the scene
                    var scenePath = $"Assets/Scenes/{_inputText}.unity";
                    var saveSuccessful = EditorSceneManager.SaveScene(scene, scenePath);
                    // Add this new scene to build.
                    if (saveSuccessful)
                    {
                        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
                        var existingScene = scenes.Find(settingsScene => settingsScene.path == scenePath);
                        if (existingScene == null)
                        {
                            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                            EditorBuildSettings.scenes = scenes.ToArray();
                        }
                        else
                        {
                            Debug.LogWarning($"Build index contains scene {scenePath} already");
                            existingScene.enabled = true;
                        }

                        Close();
                    }
                }
            }

            if (GUILayout.Button("Abort"))
            {
                Close();
            }
            EditorGUILayout.EndScrollView();
        }
    }
}