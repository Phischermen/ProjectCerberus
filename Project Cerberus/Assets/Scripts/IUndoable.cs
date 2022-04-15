using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUndoable
{
    //method instantiates StateData and returns it
    StateData GetUndoData();
}
