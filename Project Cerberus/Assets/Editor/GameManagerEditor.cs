using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(GameManager))]
    public class GameManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var gameManager = target as GameManager;
            DrawDefaultInspector();
            if (gameManager != null)
            {
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