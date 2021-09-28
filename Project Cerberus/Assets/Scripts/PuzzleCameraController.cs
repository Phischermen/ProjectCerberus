using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PuzzleCameraController : MonoBehaviour
{
    private static Vector3 _position;
    private static float _shake = 0f;
    private static float _maxShake = 1f;
    private void Awake()
    {
        _position = transform.position;
    }
    
    void Update()
    {
        transform.position = _position + new Vector3(Random.Range(-_shake, _shake), Random.Range(-_shake, _shake));
        _shake = Mathf.Max(0f, _shake - Time.deltaTime);
    }

    public static void AddShake(float shake)
    {
        _shake = Mathf.Min(_shake + shake, _maxShake);
    }

    public static void SetPosition(Vector3 position)
    {
        _position = position;
    }
}