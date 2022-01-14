/*
 * PuzzleUIEndCardFailure controls what is displayed on the failure end card, and has a button to reset the level.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleUIEndCardFailure : MonoBehaviour
{
    private GameManager _gameManager;
    [SerializeField] private Image _failureTextures;
    [SerializeField] private Text _failureMessages;

    public enum DeathPresetName
    {
        spiked,
        fellIntoPit,
        last
    }

    [Serializable]
    public class DeathPreset
    {
        public string title;
        public Sprite image;
    }

    public DeathPreset[] deathPresets;
    private void Awake()
    {
        _gameManager = FindObjectOfType<GameManager>();
        // Validate length of deathPresets
        if (deathPresets.Length > (int) DeathPresetName.last)
        {
            NZ.NotifyZach("Death Presets is too long!");
        }
    }

    // Button actions
    public void Retry()
    {
        _gameManager.ReplayLevel();
        // TODO Play animation to hide UI instead of imediately destroying it.
        Destroy(gameObject);
    }

    public void UndoLastMove()
    {
        _gameManager.UndoLastMove();
        Destroy(gameObject);
    }

    public void SetFieldsToDeathPreset(DeathPresetName deathPresetName)
    {
        //TODO Get refrences to text and image components so that this can be implemented.
        // Set UI fields to deathPreset.
        int deathIndex = (int)(deathPresetName);
        DeathPreset typeOfDeath = deathPresets[deathIndex];

        // Set fields for images.
        _failureTextures.sprite = typeOfDeath.image;
        _failureMessages.text = typeOfDeath.title;
    }
}
