using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "DialogueDatabaseAsset")]
public class DialogueDatabaseAsset : ScriptableObject
{
    public enum Speaker
    {
        jack,
        kahuna,
        laguna,
        hades
    }

    [Serializable]
    public class Scene
    {
        public bool expandedInInspector;
        public string sceneTitle = "";
        public List<Dialogue> dialogues = new List<Dialogue>();
    }
    [Serializable]
    public class Dialogue
    {
        public string id = "";
        public Speaker speaker;
        public string line = "";

        public void GenerateId()
        {
            id = $"{(char)Random.Range('A','Z')}{Random.Range(0,9)}{(char)Random.Range('A','Z')}{Random.Range(0,9)}";
        }
    }

    public List<Scene> scenes;
    
}
