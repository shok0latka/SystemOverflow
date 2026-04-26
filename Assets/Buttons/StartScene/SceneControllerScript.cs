using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    [SerializeField] private int _wasStarted = 0;
    [SerializeField] private GameObject _buttonContinue;
    [SerializeField] private GameObject _buttonStart;
    [SerializeField] private GameObject _buttonSettings;
    void Start()
    {
        LoadGame();
        UpdateButtons();
    }

    void Update()
    {
        
    }

    void LoadGame()
    {
        if (PlayerPrefs.HasKey("WasStarted"))
        {
            _wasStarted = PlayerPrefs.GetInt("WasStarted");
            Debug.Log("Game data loaded!");
        }
        else
        {
            Debug.LogError("There is no save data!");
        }    
    }

    void UpdateButtons() => _buttonContinue.SetActive(_wasStarted != 0);
}
