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
        private BonusStar _bonusStar;

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
                gameManager.maxMovesBeforeStarLoss =
                    EditorGUILayout.IntSlider("Max moves before star loss", gameManager.maxMovesBeforeStarLoss, 1, 200);
            }

            // Add toggle to set no time limit.
            gameManager.infiniteParTime =
                EditorGUILayout.Toggle("Infinite Par Time", gameManager.infiniteParTime);
            if (!gameManager.infiniteParTime)
            {
                // If there will be a time limit, add slider to set that time limit.
                gameManager.parTime = Mathf.Floor(
                    EditorGUILayout.Slider("Par Time (sec)", gameManager.parTime, 1, 240));
            }

            // Add BonusStar's controls
            EditorGUILayout.LabelField("Controls for bonus star:");
            if (_bonusStar)
            {
                _bonusStar.DrawControlsForInspectorGUI();
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "No object of type 'BonusStar' found in scene. Controls for that BonusStar will appear here. Player will be rewarded one star rating for free if level has no BonusStar.",
                    MessageType.Warning);
                _bonusStar = FindObjectOfType<BonusStar>();
            }

            // Apply changes.
            if (GUI.changed)
            {
                EditorUtility.SetDirty(gameManager);
                EditorUtility.SetDirty(_bonusStar);
            }
        }
    }
}