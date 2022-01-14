/*
 * This is a custom asset that specifies the order our levels should be played in. It includes a find function that can
 * return the level sequence number where the scene is located. The active level sequence can be set via the Project
 * Settings under "Custom Project Settings > Main Sequence".
 */

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "LevelSequence")]
public class LevelSequence : ScriptableObject
{
    [Serializable]
    public class World
    {
        // public List<SceneAsset> levels;
        public List<int> levels;
        public bool expandedInInspector;

        public World()
        {
            expandedInInspector = true;
            // levels = new List<SceneAsset>();
            levels = new List<int>();
        }
    }

    public List<World> worlds;

    private void OnEnable()
    {
        if (worlds == null)
        {
            worlds = new List<World>();
        }
    }

    public void AddWorld()
    {
        // Initialize world.
        var world = new World();
        // Sanity check.
        if (worlds == null)
        {
            worlds = new List<World>();
        }

        worlds.Add(world);
    }

    public int FindCurrentLevelSequence(int sceneBuildIndex)
    {
        var numberOfLevelsInPreviousWorld = 0;
        // Search through each world's level list for scene via build index.
        for (int i = 0; i < worlds.Count; i++)
        {
            var levelSequence = worlds[i].levels.FindIndex((idx) => idx == sceneBuildIndex);
            if (levelSequence == -1)
            {
                numberOfLevelsInPreviousWorld += worlds[i].levels.Count;
            }
            else
            {
                return levelSequence + numberOfLevelsInPreviousWorld;
            }
        }

        // Level not found.
        return -1;
    }

    public int GetSceneBuildIndexForLevel(int levelSequence)
    {
        // Subtract number of levels in each world until levelSequence is less than the number of levels in the current
        // world. Index that world for the scene build index.
        for (int i = 0; i < worlds.Count; i++)
        {
            var levelsInCurrentWorld = worlds[i].levels;
            if (levelSequence >= levelsInCurrentWorld.Count)
            {
                levelSequence -= levelsInCurrentWorld.Count;
            }
            else if (levelSequence > 0)
            {
                return levelsInCurrentWorld[levelSequence];
            }
        }

        // Scene not found.
        return -1;
    }
}