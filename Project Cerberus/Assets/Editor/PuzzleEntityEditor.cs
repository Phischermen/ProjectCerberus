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
        }

        private void CreateSfxField(string label, ref AudioSource source)
        {
            source = (AudioSource) EditorGUILayout.ObjectField(label, source, typeof(AudioSource), true);
        }
    }
}