/*
 * A custom editor for DialogueDatabaseAsset
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(DialogueDatabaseAsset))]
    public class DialogueDatabaseAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var dialogueDatabase = (DialogueDatabaseAsset) target;
            GUILayout.Label("Dialogue Database");
            // Iterate through every scene.
            for (int i = 0; i < dialogueDatabase.scenes.Count; i++)
            {
                var scene = dialogueDatabase.scenes[i];
                
                EditorGUILayout.BeginHorizontal();
                // Add title text area
                scene.sceneTitle = EditorGUILayout.TextField(scene.sceneTitle);
                // Add move up button
                if (GUILayout.Button("▲") && i != 0)
                {
                    // Make move up undoable.
                    UnityEditor.Undo.RecordObject(dialogueDatabase, "Move Up");
                    // Move the dialogue "up" the list.
                    dialogueDatabase.scenes.RemoveAt(i);
                    dialogueDatabase.scenes.Insert(i - 1, scene);
                }

                // Add move down button.
                if (GUILayout.Button("▼") && i != dialogueDatabase.scenes.Count - 1)
                {
                    // Make move down undoable.
                    UnityEditor.Undo.RecordObject(dialogueDatabase, "Move Down");
                    // Move the dialogue "down" the list.
                    dialogueDatabase.scenes.RemoveAt(i);
                    dialogueDatabase.scenes.Insert(i + 1, scene);
                }

                // Add insert button.
                if (GUILayout.Button("Ins"))
                {
                    // Make insert undoable.
                    UnityEditor.Undo.RecordObject(dialogueDatabase, "Insert Scene");
                    // Create new dialogue and insert at the current index.
                    var newScene = new DialogueDatabaseAsset.Scene();
                    dialogueDatabase.scenes.Insert(i, newScene);
                }

                // Add delete button.
                if (GUILayout.Button("✖"))
                {
                    // Make delete undoable.
                    UnityEditor.Undo.RecordObject(dialogueDatabase, "Delete Scene");
                    // Remove the scene
                    dialogueDatabase.scenes.RemoveAt(i);
                }

                EditorGUILayout.EndHorizontal();
                scene.expandedInInspector =
                    EditorGUILayout.BeginFoldoutHeaderGroup(scene.expandedInInspector, "Dialogues");
                if (scene.expandedInInspector)
                {
                    // Iterate through every dialogue.
                    for (var j = 0; j < scene.dialogues.Count; j++)
                    {
                        var dialogue = scene.dialogues[j];
                        EditorGUILayout.BeginHorizontal();
                        dialogue.id = GUILayout.TextField(dialogue.id, 4);
                        dialogue.speaker = (DialogueDatabaseAsset.Speaker) EditorGUILayout.EnumPopup(dialogue.speaker);
                        // Add move up button
                        if (GUILayout.Button("▲") && j != 0)
                        {
                            // Make move up undoable.
                            UnityEditor.Undo.RecordObject(dialogueDatabase, "Move Up");
                            // Move the dialogue "up" the list.
                            scene.dialogues.RemoveAt(j);
                            scene.dialogues.Insert(j - 1, dialogue);
                        }

                        // Add move down button.
                        if (GUILayout.Button("▼") && j != scene.dialogues.Count - 1)
                        {
                            // Make move down undoable.
                            UnityEditor.Undo.RecordObject(dialogueDatabase, "Move Down");
                            // Move the dialogue "down" the list.
                            scene.dialogues.RemoveAt(j);
                            scene.dialogues.Insert(j + 1, dialogue);
                        }

                        // Add insert button.
                        if (GUILayout.Button("Ins"))
                        {
                            // Make insert undoable.
                            UnityEditor.Undo.RecordObject(dialogueDatabase, "Insert Scene");
                            // Create new dialogue and insert at the current index.
                            var newDialogue = new DialogueDatabaseAsset.Dialogue();
                            newDialogue.GenerateId();
                            scene.dialogues.Insert(j, newDialogue);
                        }

                        // Add delete button.
                        if (GUILayout.Button("✖"))
                        {
                            // Make delete undoable.
                            UnityEditor.Undo.RecordObject(dialogueDatabase, "Delete Scene");
                            // Remove the scene
                            scene.dialogues.RemoveAt(j);
                        }

                        EditorGUILayout.EndHorizontal();
                        dialogue.line = EditorGUILayout.TextArea(dialogue.line);
                    }

                    // Add dialogue button
                    if (GUILayout.Button("Add Dialogue"))
                    {
                        // Make add dialogue undoable.
                        UnityEditor.Undo.RecordObject(dialogueDatabase, "Add Scene");
                        // Create new dialogue.
                        var dialogue = new DialogueDatabaseAsset.Dialogue();
                        dialogue.GenerateId();
                        scene.dialogues.Add(dialogue);
                    }
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            // Add scene button
            if (GUILayout.Button("Add Scene"))
            {
                // Make add dialogue undoable.
                UnityEditor.Undo.RecordObject(dialogueDatabase, "Add Scene");
                // Create new dialogue.
                var scene = new DialogueDatabaseAsset.Scene();
                dialogueDatabase.scenes.Add(scene);
            }
            
            // Generate Dialogue Database
            if (GUILayout.Button("Generate CSharp script"))
            {
                var fileContents = new List<string>();
                fileContents.Add("// This file was generated by DialogueDatabaseAssetEditor.cs. Do not edit!");
                fileContents.Add("using UnityEngine;");
                fileContents.Add("public static class DialogueDatabase");
                fileContents.Add("{");
                for (var i = 0; i < dialogueDatabase.scenes.Count; i++)
                {
                    var scene = dialogueDatabase.scenes[i];
                    fileContents.Add($"\t//{scene.sceneTitle}");
                    for (var j = 0; j < scene.dialogues.Count; j++)
                    {
                        var dialogue = scene.dialogues[j];
                        fileContents.Add($"\tpublic static Vector2Int {dialogue.id} = new Vector2Int({i},{j});");
                    }
                }

                fileContents.Add("}");
                var scriptPath = Application.dataPath + "/Scripts/DialogueDatabase.cs";
                File.WriteAllLines(scriptPath, fileContents);
                AssetDatabase.ImportAsset("Assets/Scripts/DialogueDatabase.cs");
                Debug.Log("DialogueDatabase Generated!");
            }

            if (GUI.changed)
            {
                // Apply changes.
                EditorUtility.SetDirty(dialogueDatabase);
            }
        }
    }
}