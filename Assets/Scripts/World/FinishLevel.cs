using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLevel : Interactable
{
    public GameObject _fade;
    [SerializeField] string NextScene;
    public override void Interact()
    {
        Destroy(gameObject);
        _fade.GetComponent<SceneTransition>().ChangeScene(NextScene);
    }
}