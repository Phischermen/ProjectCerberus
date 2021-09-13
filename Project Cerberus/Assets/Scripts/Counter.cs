using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Counter : MonoBehaviour, IUndoable
{
    public class CounterUndoData : UndoData
    {
        private Counter _counter;
        private int _count;
        public CounterUndoData(Counter counter, int count)
        {
            _counter = counter;
            _count = count;
        }

        public override void Load()
        {
            _counter.count = _count;
        }
    }
    
    [SerializeField] public int initialValue;
    [HideInInspector] public int count;

    public UnityEvent OnCounterEqualsZero;
    public UnityEvent OnCounterNoLongerEqualsZero;

    #if DEBUG
    private void OnGUI()
    {
        var screenPoint = Camera.main.WorldToScreenPoint(transform.position);
        var content = new GUIContent($"{name}\nCount: {count}");
        GUIStyle style = GUI.skin.box;
        var size = style.CalcSize(content);
        GUI.Box(new Rect(screenPoint.x, Screen.height - screenPoint.y, size.x,size.y), content);
    }
    #endif
    private void Awake()
    {
        count = initialValue;
    }

    public void IncrementCounter()
    {
        var wasZero = count == 0;
        count += 1;
        if (wasZero)
        {
            OnCounterNoLongerEqualsZero.Invoke();
        }
        else if (count == 0)
        {
            OnCounterEqualsZero.Invoke();
        }
    }

    public void DecrementCounter()
    {
        var wasZero = count == 0;
        count -= 1;
        if (wasZero)
        {
            OnCounterNoLongerEqualsZero.Invoke();
        }
        else if (count == 0)
        {
            OnCounterEqualsZero.Invoke();
        }
    }

    public UndoData GetUndoData()
    {
        var undoData = new CounterUndoData(this, count);
        return undoData;
    }

}