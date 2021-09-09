using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUndoable
{
    //method instantiates UndoData and returns it
    UndoData GetUndoData();


}
