﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicBlock : PuzzleEntity
{
    protected BasicBlock()
    {
        isBlock = true;
        stopsBlock = true;
        stopsPlayer = true;
        pushableByFireball = true;
        pushable = true;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
