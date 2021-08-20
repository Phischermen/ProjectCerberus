using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleUI : MonoBehaviour
{
    public Text turnCounter;
    public Text currentDog;
    public Text secondDog;
    public Text thirdDog;
    
    private GameManager _manager;
    private int index;

    void Awake()
    {
        _manager = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        // Write some code here to update Menu UI - Samuel

        turnCounter.text = $"Turn:\n{_manager.turn}";
        



        // Iterate over each move, returning the current and next dog.
        foreach (Cerberus current in _manager.moveOrder)
        {
            // Current dog: Jack
            Debug.Log("Current dog: " + current.name);

            if (_manager.moveOrder[_manager.currentMove].name == "Jack")
            {
                currentDog.text = _manager.moveOrder[0].name;
                secondDog.text = _manager.moveOrder[1].name;
                thirdDog.text = _manager.moveOrder[2].name;
            }

            else if (_manager.moveOrder[_manager.currentMove].name == "Kahuna")
            {
                currentDog.text = _manager.moveOrder[1].name;
                secondDog.text = _manager.moveOrder[2].name;
                thirdDog.text = _manager.moveOrder[0].name;
            }

            else if (_manager.moveOrder[_manager.currentMove].name == "Laguna")
            {
                currentDog.text = _manager.moveOrder[2].name;
                secondDog.text = _manager.moveOrder[0].name;
                thirdDog.text = _manager.moveOrder[1].name;
            }
        }
    }
}
