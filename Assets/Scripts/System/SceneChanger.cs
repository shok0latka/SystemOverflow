using UnityEngine;

public class SceneChanger : MonoBehaviour
{
    public GameObject _fade;
    
    public void FirstLevel() => _fade.GetComponent<SceneTransition>().ChangeScene("FirstLevel");
}
