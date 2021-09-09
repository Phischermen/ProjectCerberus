using System;
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

        // TODO Samuel: Fill in these methods.
        public void SetUIToMovedPreset()
        {
            text.color = Color.gray;
        }

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

    public Text firstDog;
    public Text secondDog;
    public Text thirdDog;


    [SerializeField] private DogStatus[] dogStatusArray;
    private Dictionary<Type, DogStatus> _dogStatusMap;
    private Vector3[] _positionCache;

    private GameManager _manager;

    void Awake()
    {
        _manager = FindObjectOfType<GameManager>();
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

        // Cache initial positions of UI elements
        _positionCache = new Vector3[3];
        for (var i = 0; i < 3; i++)
        {
            var dogStatus = dogStatusArray[i];
            _positionCache[i] = dogStatus.rectTransform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // TODO Kevin: Move this out of Update loop into new UpdateUI method
        // Update turn counter
        turnCounter.text = $"Turns left:\n{_manager.maxTurns - _manager.turn}";
        
        // Hide all dog status initially
        foreach (var dogStatus in dogStatusArray)
        {
            dogStatus.HideUI();
        }

        // Iterate over moveOrder to update dogStatus
        for (int i = 0; i < _manager.moveOrder.Count; i++)
        {
            var cerberus = _manager.moveOrder[i];
            if (i == 0)
            {
                dogStatusArray[0].text.text = cerberus.isCerberusMajor ? "Cerberus" : "Jack";
            }
            // Get dogStatus from map
            var dogStatus = _dogStatusMap[cerberus.GetType()];
            dogStatus.ShowUI();
            // Set preset
            if (i < _manager.currentMove)
            {
                dogStatus.SetUIToMovedPreset();
            }
            else if (i == _manager.currentMove)
            {
                dogStatus.SetUIToCurrentlyControlledPreset();
            }
            else
            {
                dogStatus.SetUIToYetToMovePreset();
            }

            // Set transform
            dogStatus.rectTransform.position = _positionCache[i];
        }
    }
}