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
    }
}