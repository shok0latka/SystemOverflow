using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PC : Interactable
{
    public string _scratchScene = "SO Script UI";
    public override void Interact()
    {
        Debug.Log("[b[b]]");
        SceneManager.LoadScene(_scratchScene);
    } 
}
