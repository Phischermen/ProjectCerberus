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

            if (GUILayout.Button("Set Next Scene"))
            {
                GenericMenu menu = new GenericMenu();
                foreach (var scene in EditorBuildSettings.scenes)
                {
                    var name = scene.path;
                    name = name.Remove(name.LastIndexOf('.'));
                    name = name.Remove(0, name.LastIndexOf('/') + 1);
                    menu.AddItem(new GUIContent(name), gameManager.nextScene.Equals(name), OnSelectScene, name);
                }

                menu.ShowAsContext();
            }

            gameManager.infinteMovesTilStarLoss =
                EditorGUILayout.Toggle("Infinite Turns", gameManager.infinteMovesTilStarLoss);
            if (!gameManager.infinteMovesTilStarLoss)
            {
                gameManager.maxMovesUntilStarLoss =
                    EditorGUILayout.IntSlider("Max Turns", gameManager.maxMovesUntilStarLoss, 1, 50);
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(gameManager);
            }
        }

        private void OnSelectScene(object scene)
        {
            var gameManager = (GameManager) target;
            gameManager.nextScene = (string) scene;
            EditorUtility.SetDirty(target);
        }
    }
}