using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Editor
{
    [CustomEditor(typeof(PuzzleEntity), true)]
    public class PuzzleEntityEditor : UnityEditor.Editor
    {
        private SerializedProperty onStandardPushed;
        private SerializedProperty onSuperPushed;
        private SerializedProperty onMultiPushed;
        private SerializedProperty onHitByFireball;
        private SerializedProperty onPulled;

        public void OnEnable()
        {
            onStandardPushed = serializedObject.FindProperty(nameof(PuzzleEntity.onStandardPushed));
            onSuperPushed = serializedObject.FindProperty(nameof(PuzzleEntity.onSuperPushed));
            onMultiPushed = serializedObject.FindProperty(nameof(PuzzleEntity.onMultiPushed));
            onHitByFireball = serializedObject.FindProperty(nameof(PuzzleEntity.onHitByFireball));
            onPulled = serializedObject.FindProperty(nameof(PuzzleEntity.onPulled));
        }

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
            entity.showOptionalSfx =
                EditorGUILayout.BeginFoldoutHeaderGroup(entity.showOptionalSfx, "Optional Sound Effects");
            if (entity.showOptionalSfx)
            {
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

            EditorGUILayout.EndFoldoutHeaderGroup();
            // Events
            entity.showOptionalEvents =
                EditorGUILayout.BeginFoldoutHeaderGroup(entity.showOptionalEvents, "Optional Events");
            if (entity.showOptionalEvents)
            {
                if (entity.pushableByStandardMove)
                {
                    CreateEventField(onStandardPushed);
                }

                if (entity.pushableByJacksSuperPush)
                {
                    CreateEventField(onSuperPushed);
                }

                if (entity.pushableByJacksMultiPush)
                {
                    CreateEventField(onMultiPushed);
                }

                if (entity.pushableByFireball || entity.interactsWithFireball)
                {
                    CreateEventField(onHitByFireball);
                }

                if (entity.pullable)
                {
                    CreateEventField(onPulled);
                }
            }
            
            // Apply changes
            if (GUI.changed)
            {
                EditorUtility.SetDirty(entity);
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void CreateSfxField(string label, ref AudioSource source)
        {
            source = (AudioSource) EditorGUILayout.ObjectField(label, source, typeof(AudioSource), true);
        }

        private void CreateEventField(SerializedProperty property)
        {
            EditorGUILayout.PropertyField(property);
        }
    }
}