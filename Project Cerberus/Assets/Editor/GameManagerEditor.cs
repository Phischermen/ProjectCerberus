/*
 * Custom inspector for GameManager.
 */
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(GameManager))]
    public class GameManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var gameManager = (GameManager) target;
            DrawDefaultInspector();
            // Display value of nextLevel.
            GUILayout.Label($"Next Level: {gameManager.nextScene}");
            // Add button to set nextLevel.
            if (GUILayout.Button("Set Next Scene"))
            {
                // Initialize generic menu.
                GenericMenu menu = new GenericMenu();
                // Populate generic menu with scene names from EditorBuildSettings.
                foreach (var scene in EditorBuildSettings.scenes)
                {
                    // Get scene path
                    var name = scene.path;
                    // Extract name from the full path.
                    name = name.Remove(name.LastIndexOf('.'));
                    name = name.Remove(0, name.LastIndexOf('/') + 1);
                    // Add the item to the menu. OnSelectScene is the callback.
                    menu.AddItem(new GUIContent(name), gameManager.nextScene.Equals(name), OnSelectScene, name);
                }
                // Show menu.
                menu.ShowAsContext();
            }
            // Add toggle to set no limit on moves until star loss.
            gameManager.infinteMovesTilStarLoss =
                EditorGUILayout.Toggle("No Move Limit For Star Loss", gameManager.infinteMovesTilStarLoss);
            if (!gameManager.infinteMovesTilStarLoss)
            {
                // If there will be a limit, add int slider to set that limit.
                gameManager.maxMovesUntilStarLoss =
                    EditorGUILayout.IntSlider("Moves until star loss", gameManager.maxMovesUntilStarLoss, 1, 50);
            }
            // Add toggle to set no time limit.
            gameManager.infiniteTimeLimit =
                EditorGUILayout.Toggle("Infinite Time Limit", gameManager.infiniteTimeLimit);
            if (!gameManager.infiniteTimeLimit)
            {
                // If there will be a time limit, add slider to set that time limit.
                gameManager.timeLimit =
                    EditorGUILayout.Slider("Time Limit (sec)", gameManager.timeLimit, 1, 120);
            }
            // Apply changes.
            if (GUI.changed)
            {
                EditorUtility.SetDirty(gameManager);
            }
        }

        private void OnSelectScene(object scene)
        {
            // Set next scene
            var gameManager = (GameManager) target;
            gameManager.nextScene = (string) scene;
            // Apply change
            EditorUtility.SetDirty(target);
        }
    }
}