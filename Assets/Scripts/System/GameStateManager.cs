using UnityEditor.Purchasing;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;

    public GameObject pause;

    private bool _isPaused;

    private Checkpoint _currentCheckpoint;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        _isPaused = !_isPaused;
        pause.SetActive(_isPaused);
        Time.timeScale = _isPaused ? 0f : 1f;
    }

    public void WinLevel()
    {
        Debug.Log("Уровень завершён");
    }

    public void StartSceneProblem()
    {
        _isPaused = false;
        pause.SetActive(false);
        Time.timeScale = 1f;
    }

    public void UpdateCheckpoint(Checkpoint newCheckpoint)
    {
        _currentCheckpoint = newCheckpoint;
        Debug.Log("Новый чекпоинт активирован!");
    }

    public void RespawnPlayer(PlayerHealth playerHealth)
    {
        if (_currentCheckpoint != null)
        {
            GameObject player = playerHealth.gameObject;
            player.transform.position = _currentCheckpoint.RespawnPosition;

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }
            
            playerHealth.SetCurrentHp(playerHealth.MaxHp);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
