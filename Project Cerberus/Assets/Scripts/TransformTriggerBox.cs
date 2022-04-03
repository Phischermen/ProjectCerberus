/*
 * TransformTriggerBox scans for transforms of puzzle entities stored in objectsToScanFor. It is more expensive than
 * PuzzleTriggerBox, but it is more convenient for certain scenarios, such as scanning an area that spans multiple cells.
 * When TransformTriggerBox finds an entity its scanning for, it invokes 'OnTriggered.' 
 */
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class TransformTriggerBox : MonoBehaviour
{
    public List<PuzzleEntity> objectsToScanFor = new List<PuzzleEntity>();
    public UnityEvent onTrigger;
    public bool mustContainAllToTrigger;
    public int triggerThreshold;
    private bool _tripped;

    public Bounds bounds;

    private void Start()
    {
        if (mustContainAllToTrigger)
        {
            triggerThreshold = objectsToScanFor.Count;
        }
    }
    
    void Update()
    {
        var transformsInBounds = objectsToScanFor.Count(entity => bounds.Contains(entity.transform.position));
        if (!_tripped && transformsInBounds >= triggerThreshold)
        {
            _tripped = true;
            onTrigger.Invoke();
        }
        else if (_tripped && transformsInBounds < triggerThreshold)
        {
            _tripped = false;
        }
    }
}