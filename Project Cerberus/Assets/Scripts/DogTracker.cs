using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogTracker : MonoBehaviour
{
    //getting the GameManager
    private GameManager manager;

    //time variables for lerp
    float t;
    float timeToReachTarget;

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
        _jack = FindObjectOfType<Jack>();
        _kahuna = FindObjectOfType<Kahuna>();
        _laguna = FindObjectOfType<Laguna>();
        
    }

    void Update()
    {
        //updates current dog by indexing moveOrder
        currentDog = manager.moveOrder[currentOrder];

        t += Time.deltaTime / timeToReachTarget;       

        
        //checks if dog is done with move and then proceeds to next index in moveOrder
        if (currentDog.doneWithMove)
        {
            currentOrder += 1;
            //resets currentOrder so it won't get out of list range
            if (currentOrder > 2)
            {
                currentOrder = 0;
            }
        }

        if (currentDog.name == "Jack")
        {
            transform.position = Vector2.Lerp(transform.position, _jack.position, t);
        }

        else if (currentDog.name == "Kahuna")
        {
            transform.position = Vector2.Lerp(transform.position, _kahuna.position, t);
        }

        else if (currentDog.name == "Laguna")
        {
            transform.position = Vector2.Lerp(transform.position, _laguna.position, t);
        }

    }

}
