using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleUI : MonoBehaviour
{
    public Text turnCounter;
    public Text currentDog;
    
    private PuzzleContainer _puzzle;

    void Awake()
    {
        _puzzle = FindObjectOfType<PuzzleContainer>();
    }

    // Update is called once per frame
    void Update()
    {
        turnCounter.text = $"Turn:\n{_puzzle.turn}";
        currentDog.text = _puzzle.moveOrder[_puzzle.currentMove].name;
    }
}
