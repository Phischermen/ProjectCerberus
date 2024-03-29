﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PuzzleUI : MonoBehaviour
{
    // Appearance of UI elements is managed through this class.

    [Serializable]
    public class DogStatus
    {
        // Fields initialized through inspector
        public RectTransform rectTransform;
        public Text text;

        public void SetUIToCurrentlyControlledPreset()
        {
            text.color = Color.red;
        }

        public void SetUIToYetToMovePreset()
        {
            text.color = Color.white;
        }

        public void HideUI()
        {
            text.gameObject.SetActive(false);
        }

        public void ShowUI()
        {
            text.gameObject.SetActive(true);
        }
    }

    public Text turnCounter;
    public Text timeCounter;
    public Text bonusStarLabel;

    public Text firstDog;
    public Text secondDog;
    public Text thirdDog;
    public Text mergeButton;
    private string toMerge = "Merge";
    private string toSplit = "Split";


    [SerializeField] private DogStatus[] dogStatusArray;
    private Dictionary<Type, DogStatus> _dogStatusMap;
    private Vector3[] _positionCache;

    private GameManager _manager;
    private BonusStar _bonusStar;

    void Awake()
    {
        _manager = FindObjectOfType<GameManager>();
        _bonusStar = FindObjectOfType<BonusStar>();
        // Check length of dogStatusArray
        if (dogStatusArray.Length != 3)
        {
            NZ.NotifyZach("dogStatusArray is not the right size");
        }

        // Initialize dictionary
        _dogStatusMap = new Dictionary<Type, DogStatus>();
        _dogStatusMap.Add(typeof(Jack), dogStatusArray[0]);
        _dogStatusMap.Add(typeof(Kahuna), dogStatusArray[1]);
        _dogStatusMap.Add(typeof(Laguna), dogStatusArray[2]);
        _dogStatusMap.Add(typeof(CerberusMajor), dogStatusArray[0]);

        // Cache initial local positions of UI elements
        _positionCache = new Vector3[3];
        for (var i = 0; i < 3; i++)
        {
            var dogStatus = dogStatusArray[i];
            _positionCache[i] = dogStatus.rectTransform.localPosition;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Update turn counter
        turnCounter.text = _manager.move.ToString();
        timeCounter.text = _manager.timer.ToString("F1");
        if (_bonusStar != null)
        {
            bonusStarLabel.text = _bonusStar.GetStatusMessageForUI();
        }

        // Hide all dog status initially
        foreach (var dogStatus in dogStatusArray)
        {
            dogStatus.HideUI();
        }

        // Iterate over moveOrder to update dogStatus
        for (int i = 0; i < _manager.availableCerberus.Count; i++)
        {
            var cerberus = _manager.availableCerberus[i];
            if (i == 0)
            {
                // Set text based on merged status

                mergeButton.text = $"Left Ctrl: {(cerberus.isCerberusMajor ? toSplit : toMerge)}";
            }

            // Get dogStatus from map.
            var dogStatus = _dogStatusMap[cerberus.GetType()];
            dogStatus.ShowUI();
            // Set text.
            dogStatus.text.text = cerberus.name;
            // Set preset.
            if (cerberus == _manager.currentCerberus)
            {
                dogStatus.SetUIToCurrentlyControlledPreset();
            }
            else
            {
                dogStatus.SetUIToYetToMovePreset();
            }

            // Set transform
            dogStatus.rectTransform.localPosition = _positionCache[i];
        }
    }
}