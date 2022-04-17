using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Counter : MonoBehaviour, IUndoable
{
    public class CounterStateData : StateData
    {
        private Counter _counter;
        public byte count => myByte;
        public CounterStateData(Counter counter, int count)
        {
            _counter = counter;
            // NOTE: In order not to ruin serialization of counter, I am not refactoring the type of "count."
            // Preserve sign by first casting to a signed 8bit int and then to unsigned 8bit.
            myByte = (byte) (sbyte)count;
        }

        public override void Load()
        {
            // Restore sign of count.
            _counter.count = (sbyte)count;
        }
    }
    
    [SerializeField] public int initialValue;
    [HideInInspector] public int count;

    private static float _textPopupDelayIncrement = 0.1f;
    private float _textPopupDelay;
    
    public UnityEvent OnCounterEqualsZero;
    public UnityEvent OnCounterNoLongerEqualsZero;
    private HashSet<MonoBehaviour> _targets;

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
        _targets = new HashSet<MonoBehaviour>();
        // Cache all of the persistent targets for this class's UnityEvents.
        AddPersistentTargetsToTargets(OnCounterEqualsZero);
        AddPersistentTargetsToTargets(OnCounterNoLongerEqualsZero);
    }

    private void Update()
    {
        _textPopupDelay = 0f;
    }

    public void IncrementCounter()
    {
        var wasZero = count == 0;
        count += 1;
        CreateTextPopupForAllEventListeners();
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
        CreateTextPopupForAllEventListeners();
        if (wasZero)
        {
            OnCounterNoLongerEqualsZero.Invoke();
        }
        else if (count == 0)
        {
            OnCounterEqualsZero.Invoke();
        }
    }

    private void AddPersistentTargetsToTargets(UnityEvent @event)
    {
        for (int i = 0; i < @event.GetPersistentEventCount(); i++)
        {
            var listener = @event.GetPersistentTarget(i);
            if (listener is MonoBehaviour behaviour)
            {
                _targets.Add(behaviour);
            }
        }
    }
    
    private void CreateTextPopupForAllEventListeners()
    {
        foreach (var listener in _targets)
        {
            var popup = TextPopup.Create(Mathf.Abs(count).ToString(), Color.red);
            popup.transform.position = listener.transform.position;
            popup.PlayRiseAndFadeAnimation(_textPopupDelay);
            _textPopupDelay += _textPopupDelayIncrement;
        }
    }

    public StateData GetUndoData()
    {
        var undoData = new CounterStateData(this, count);
        return undoData;
    }
}