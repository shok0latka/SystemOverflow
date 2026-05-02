using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Interactable
{
    public override void Interact()
    {
        Debug.Log("Door is open");
        Destroy(gameObject);
    }
}
