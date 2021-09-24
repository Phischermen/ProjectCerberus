/*
 * This is a custom asset that specifies the order our levels should be played in. It includes a find function that
 * returns both the level and the world where the scene is located. The active level sequence can be set via the Project
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
        public List<SceneAsset> levels;
        public bool expandedInInspector;

        public World()
        {
            expandedInInspector = true;
            levels = new List<SceneAsset>();
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

    public void FindCurrentLevelAndWorld(string searchName, out int level, out int world)
    {
        // Set to -1 to indicate scene was not found.
        world = -1;
        level = -1;
        // Search through each world's level list for scene via name.
        for (int i = 0; i < worlds.Count; i++)
        {
            world = i;
            level = worlds[i].levels.FindIndex((asset) => asset.name == searchName);
            // If level found, break.
            if (level != -1) break;
        }
    }

    public int GetNumberOfWorlds()
    {
        return worlds.Count;
    }

    public int GetNumberOfLevelsInWorld(int world)
    {
        // Bound check. Return -1 to indicate out of bounds.
        if (world < 0 || world > worlds.Count) return -1;
        return worlds[world].levels.Count;
    }
    
    public SceneAsset GetLevel(int world, int level)
    {
        // Bound check. Return null to indicate out of bounds.
        if (world < 0 || world > worlds.Count) return null;
        if (level < 0 || level > worlds[world].levels.Count) return null;
        return worlds[world].levels[level];
    }
}

