using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PuzzleEntity : MonoBehaviour
{

    public bool stopsPlayer { get; protected set; }
    public bool pushable { get; protected set; }

    public abstract void OnEnterCollisionWithEntity(PuzzleEntity other);
    public abstract void OnExitCollisionWithEntity(PuzzleEntity other);
}
