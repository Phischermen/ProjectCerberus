using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pointer : MonoBehaviour
{
    public Vector2 position;
    private static float timeScale = 4f;
    private static float maxOffset = 0.25f;

    // Start is called before the first frame update
    void Start()
    {
        position = transform.position;
    }

    private void Update()
    {
        var offset = Mathf.Abs(Mathf.Sin(Time.time * timeScale)) * maxOffset;
        transform.position = position + new Vector2(offset, -offset);
    }
}
