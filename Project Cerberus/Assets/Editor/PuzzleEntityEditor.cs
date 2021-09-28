using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(PuzzleEntity), true)]
    public class PuzzleEntityEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var entity = (PuzzleEntity) target;
            GUIStyle style = GUI.skin.label;
            style.wordWrap = true;
            EditorGUILayout.LabelField(entity.entityRules, style);
            DrawDefaultInspector();
            // Check to see if entity overrides OnPlayerMadeMove via reflection
            var methodInfo = entity.GetType().GetMethod(nameof(PuzzleEntity.OnPlayerMadeMove));
            if (methodInfo.DeclaringType != typeof(PuzzleEntity))
            {
                // Add slider field to set process priority.
                entity.processPriority =
                    EditorGUILayout.Slider("Process Priority", entity.processPriority, 0f, 100f);
            }

            // SFX related fields
            EditorGUILayout.LabelField("Optional Sound Effects");
            if (entity.pushableByStandardMove || entity.pushableByJacksMultiPush)
            {
                CreateSfxField("Pushed", ref entity.pushedSfx);
            }

            if (entity.pushableByStandardMove || entity.pushableByJacksMultiPush)
            {
                CreateSfxField("Super Pushed", ref entity.superPushedSfx);
            }

            if (entity.pushableByFireball)
            {
                CreateSfxField("Pushed By Fireball", ref entity.pushedByFireballSfx);
            }

            // Apply changes
            if (GUI.changed)
            {
                EditorUtility.SetDirty(entity);
            }
        }

        private void CreateSfxField(string label, ref AudioSource source)
        {
            source = (AudioSource) EditorGUILayout.ObjectField(label, source, typeof(AudioSource), true);
        }
    }
}