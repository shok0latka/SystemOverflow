using UnityEngine;

public class SceneChanger : MonoBehaviour
{
    public GameObject _fade;
    public GameObject _gameStateManager;
    public void FirstLevel() => _fade.GetComponent<SceneTransition>().ChangeScene("FirstLevel");

    public void StartScene() => _fade.GetComponent<SceneTransition>().ChangeScene("StartScene");

    public void StartSceneOnPause()
    {
        _gameStateManager.GetComponent<GameStateManager>().StartSceneProblem();
        _fade.GetComponent<SceneTransition>().ChangeScene("StartScene");
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
