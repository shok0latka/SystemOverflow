using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : Interactable
{
    public override void Interact()
    {
        Debug.Log("+1 Монета");
        Destroy(gameObject);
    }
}
