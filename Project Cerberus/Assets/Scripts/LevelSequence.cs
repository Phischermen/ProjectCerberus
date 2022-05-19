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
    public class Level
    {
        public int idxForInstancing;
        public int idxForDisplay;
        public bool isGameplay;

        public Level(int idxForInstancing, int idxForDisplay)
        {
            this.idxForInstancing = idxForInstancing;
            this.idxForDisplay = idxForDisplay;
        }
    }
    [Serializable]
    public class World
    {
        // I'd prefer to use a tuple, but tuples are not serializable in Unity.
        // X: index for instancing
        // Y: index for display
        // TODO: Add bool to flag levels that are book levels.
        public List<Level> levels;
        public AudioClip music;
        public bool expandedInInspector;

        public World()
        {
            expandedInInspector = true;
            levels = new List<Level>();
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
            var levelSequence = worlds[i].levels.FindIndex((level) => level.idxForInstancing == sceneBuildIndex);
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

    public int GetSceneBuildIndexForLevel(int levelSequence, bool andPlayMusic = false)
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
            else if (levelSequence >= 0)
            {
                // Music management is thrown in here just for its convenience.
                if (andPlayMusic) DiskJockey.PlayTrack(worlds[i].music);
                return levelsInCurrentWorld[levelSequence].idxForInstancing;
            }
        }

        // Scene not found.
        return -1;
    }
}