using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogTracker : MonoBehaviour
{
    // TODO(Grant) Add particle emitter. Try to get it to render behind dog
    // TODO Initialize location at the current dog at start of game

    //connected to undo
    // public class TrackerUndoData : UndoData
    // {
    //     public DogTracker tracker;
    //
    //     public TrackerUndoData(DogTracker tracker)
    //     {
    //         this.tracker = tracker;
    //     }
    //
    //     public override void Load()
    //     {
    //         tracker.revertTracker();
    //     }
    //
    // }

    
    
    //getting the GameManager
    private GameManager manager;

    //used to index moveOrder
    private int currentOrder;
    private Cerberus currentDog;

    //setting up the dogs
    private Laguna _laguna;
    private Jack _jack;
    private Kahuna _kahuna;


    void Start()
    {
        //finding and getting manager and the dogs
        manager = FindObjectOfType<GameManager>();
        // _jack = FindObjectOfType<Jack>();
        // _kahuna = FindObjectOfType<Kahuna>();
        // _laguna = FindObjectOfType<Laguna>();
        
    }

    void Update()
    {
        //updates current dog by indexing moveOrder
        currentDog = manager.currentCerberus;
        // currentDog = manager.moveOrder[currentOrder];

        
        //checks if dog is done with move and then proceeds to next index in moveOrder
        // if (currentDog.doneWithMove)
        // {
        //     currentOrder += 1;
        //     //resets currentOrder so it won't get out of list range
        //     if (currentOrder > 2)
        //     {
        //         currentOrder = 0;
        //     }
        // }
        
        transform.position = Vector3.Lerp(transform.position, currentDog.transform.position, Time.deltaTime * 10);
        // if (currentDog.name == "Jack")
        // {
        //     //Debug.Log("Jack");
        //     transform.position = Vector2.Lerp(transform.position, _jack.position, Time.deltaTime * 10);
        // }
        //
        // else if (currentDog.name == "Kahuna")
        // {
        //     //Debug.Log("Kahuna");
        //     transform.position = Vector2.Lerp(transform.position, _kahuna.position, Time.deltaTime * 10);
        // }
        //
        // else if (currentDog.name == "Laguna")
        // {
        //     //Debug.Log("Laguna");
        //     transform.position = Vector2.Lerp(transform.position, _laguna.position, Time.deltaTime * 10);
        // }

    }


    // public void revertTracker()
    // {
    //     currentOrder -= 1;
    // }


}
