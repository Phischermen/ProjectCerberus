using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleUI : MonoBehaviour
{
    public Text turnCounter;
    public Text currentDog;
    
    private GameManager _manager;

    void Awake()
    {
        _manager = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        // Write some code here to update Menu UI - Samuel

        turnCounter.text = $"Turn:\n{_manager.turn}";
        currentDog.text = _manager.moveOrder[_manager.currentMove].name;
    }
}
