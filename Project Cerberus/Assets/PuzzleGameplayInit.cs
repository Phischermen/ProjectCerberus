using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleGameplayInit : MonoBehaviour
{
    [SerializeField] private GameObject UIPrefab; 
    void Awake()
    {
        Instantiate(UIPrefab);
    }
}
